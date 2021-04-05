Imports System.Net

Public Class Pixiv

	' 用于操作的函数
	Public Sub DownloadTagsImages(key As String, path As String)
		Dim json As String = GetJson(TagUrl(key, 1.ToString()))
		Dim imageInfoList = GetTagImageInfoList(json)
		DownloadImage("https://i.pximg.net/img-original/img/2021/04/05/12/10/15/88949344_p0.png", path)
	End Sub

	' 用于辅助实现的函数
	Private Function GetJson(url As String) As String
		Return New WebProtocol(url, True).GetContentDocument()
	End Function
	Private Function TagUrl(key As String, p As String) As String
		'https://www.pixiv.net/ajax/search/top/{key}?lang=zh
		Dim url As String = "https://www.pixiv.net/ajax/search/artworks/{key}?word={key}&order=date_d&mode=all&p={p}&s_mode=s_tag_full&type=all&lang=zh"
		Return url.Replace("{key}", Uri.EscapeUriString(key)).Replace("{p}", p)
	End Function
	Protected Function GetTagImageInfoList(json As String) As List(Of ImageInfo)
		Dim ret As New List(Of ImageInfo)
		Dim illust As String = TextParser.ExtractOne(json, """illustManga"":", """recent""")
		illust = TextParser.ExtractOne(illust, """data"":\[", "popular")
		Dim iifo As ImageInfo
		For Each item As String In TextParser.Extract(illust, "{", "}")
			If Not (item.Contains("id") And item.Contains("title")) Then Exit For
			iifo = New ImageInfo(item)
			ret.Add(iifo)
			'Console.WriteLine(ret.Count.ToString + ".	" + iifo.Id + ":" + iifo.Name + ":" + iifo.ImageUrl)
		Next

		Return ret
	End Function
	Protected Sub DownloadImage(url As String, path As String)
		Dim wp As New WebProtocol(url)
		wp.Referer = "https://www.pixiv.net/"
		wp.Headers.Add("sec-fetch-site", "cross-site")
		'Dim day As String = If(Date.Now.Day.ToString.Length = 1, "0" + Date.Now.Day.ToString, Date.Now.Day.ToString)
		'Dim since As String = $"{Date.Now.DayOfWeek.ToString().Substring(0, 3)}, {day} {MonthName(Month(Now)).Substring(0, 3)} {Year(Now)} {If(Second(Now).ToString.Length = 1, "0" + Second(Now).ToString, Second(Now).ToString)} {If(Minute(Now).ToString.Length = 1, "0" + Minute(Now).ToString, Minute(Now).ToString)} GMT"
		'wp.Headers.Add("If-modified-since", since)
		Dim bytes As Byte() = wp.GetContentBytes()
		IO.File.WriteAllBytes(path + "1.jpg", bytes)
	End Sub
End Class

Public Structure ImageInfo
	Dim Id As String
	Dim Name As String
	Dim ImageUrl As String
	Sub New(line As String)
		Id = TextParser.ExtractOne(line, """id""""", """,")
		Name = DeEscapeUTFString(TextParser.ExtractOne(line, """title"":""", """"))
		Dim url As String = TextParser.ExtractOne(line, """url"":""", """").Replace("\/", "/")
		ImageUrl = "https://i.pximg.net/img-original/" + url.Substring(url.IndexOf("img/")).Replace("_square1200", "").Replace("_custom1200", "")
	End Sub
	Private Function DeEscapeUTFString(input As String) As String
		'Return Uri.UnescapeDataString(input)
		Dim reg As New Text.RegularExpressions.Regex("\\u([a-z]|[0-9]){4}")
		Dim code As Integer
		Dim sb As New Text.StringBuilder(input)
		For Each chItem As Text.RegularExpressions.Match In reg.Matches(input)
			code = CInt("&H" + chItem.Value.Replace("\u", ""))
			sb.Replace(chItem.Value, Char.ConvertFromUtf32(code))
		Next
		Return sb.ToString
	End Function
End Structure
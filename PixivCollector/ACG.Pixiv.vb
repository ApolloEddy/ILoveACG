Imports System.Net

Public Class Pixiv

	Public Const Host As String = "https://www.pixiv.net/"
	Public ReadOnly Property Downloaded As List(Of ImageInfo)
		Get
			Return _downloaded
		End Get
	End Property
	Protected _downloaded As New List(Of ImageInfo)
	Public ReadOnly Property FailToDownloaded As List(Of ImageInfo)
		Get
			Return _failToDownloaded
		End Get
	End Property
	Protected _failToDownloaded As New List(Of ImageInfo)
	Protected ReadOnly Rand As New Random(CLng($"{Minute(Now)}{Second(Now)}{Date.Now.Millisecond}"))
	' 用于操作的函数


	' 用于辅助实现的函数
	Protected Function GetJson(url As String) As String
		Dim web As New WebProtocol(url, True)
		With web.Headers
			.Add("sec-fetch-dest", "empty")
			.Add("sec-fetch-mode", "cors")
			.Add("sec-fetch-site", "same-origin")
			.Add("x-userid", "65107909")
		End With
		Return web.GetContentDocument()
	End Function
	Protected Overloads Sub DownloadImage(url As String, path As String)
		Dim wp As New WebProtocol(url)
		wp.Referer = "https://www.pixiv.net/"
		wp.Headers.Add("sec-fetch-site", "cross-site")
		'wp.Headers.Add("If-modified-since", since)
		Dim bytes As Byte() = wp.GetContentBytes()
		IO.File.WriteAllBytes(path + "catched.jpg", bytes)
	End Sub
	Protected Overloads Function DownloadImage(iInfo As ImageInfo, path As String) As Integer
		Dim searchUrl As String = "https://www.pixiv.net/ajax/illust/{id}/pages?lang=zh"
		Dim pageJson = GetJson(searchUrl.Replace("{id}", iInfo.Id)) ' 谁能想一个页面里面多张图害得我还要改代码QwQ
		Dim current As Integer = 0
		Console.Write($"正在下载图片[{iInfo.Name} (id:{iInfo.Id})]...")
		For Each url As String In ExtractAllImageLinks(pageJson)
			Dim wp As New WebProtocol(url)
			wp.Referer = "https://www.pixiv.net/"
			wp.Headers.Add("sec-fetch-site", "cross-site")
			'wp.Headers.Add("If-modified-since", since)
			Dim bytes As Byte()
			For i = 1 To 6
				Try
					bytes = wp.GetContentBytes()
					Exit For
				Catch ex As Exception
					If Not i = 6 Then
						If i = 1 Then Console.WriteLine()
						PutsError($"下载失败，即将进行第 [{i + 1}] 次重试...")
					Else
						PutsError($"id为[{iInfo.Id}]的图片下载失败，错误信息：{ex.Message}")
						_failToDownloaded.Add(iInfo)
					End If
					Threading.Thread.Sleep(Rand.Next(10, 30)) ' 随机数暂停，防止被反爬机制封杀
				End Try
			Next
			current += 1
			Dim p As String = $"{path}[{iInfo.Id}]{iInfo.Name}_p{current}.jpg"
			For Each c As Char In IO.Path.GetInvalidPathChars()
				If p.Contains(c) Then p.Replace(iInfo.Name, "¿") : Exit For
			Next
			p = $"{path}[{iInfo.Id}]_p{current}.jpg"
#Disable Warning
			IO.File.WriteAllBytes(p, bytes)
#Enable Warning
		Next
		Downloaded.Add(iInfo)
		Console.Write("完成！")
		Return current
	End Function
	Protected Overloads Sub DownloadImageList(infoList As List(Of ImageInfo), path As String)
		Dim pCount As Integer = 0
		For Each img As ImageInfo In infoList
			pCount += DownloadImage(img, path)
			Console.WriteLine($"当前进度：{infoList.IndexOf(img)}/{infoList.Count}  ==> 合{Format(infoList.IndexOf(img) * 100 / infoList.Count)}%")
			Console.Title = $"{My.Application.Info.Title}  正在下载：{Format(infoList.IndexOf(img) * 100 / infoList.Count)}%"
		Next
		Console.Title = $"{My.Application.Info.Title}%"
		Console.WriteLine($"下载完毕，成功下载[{pCount}]张图片，失败[{FailToDownloaded.Count}]组图片")
	End Sub
	Protected Function ExtractAllImageLinks(json As String) As String()
		Dim ret As New List(Of String)
		For Each item As String In TextParser.Extract(json.Replace("\/", "/"), """original"":""", """}")
			ret.Add(item)
		Next
		Return ret.ToArray
	End Function
	Protected Shared Function GenerateImageUrlFromCacheUrl(cacheUrl As String) As String
		Return "https://i.pximg.net/img-original/" + cacheUrl.Substring(cacheUrl.IndexOf("img/")).Replace("_square1200", "").Replace("_custom1200", "").Replace("_master1200", "")
	End Function
	Protected Shared Sub PutsError(message As String)
		Console.ResetColor()
		Console.ForegroundColor = ConsoleColor.Red
		Console.WriteLine($"[Error] {message}")
		Console.ResetColor()
	End Sub

	Public Structure ImageInfo
		Dim Id As String
		Dim Name As String
		Dim ImageUrl As String
		Sub New(line As String)
			Id = TextParser.ExtractOne(line, """id"":""", """,")
			Name = DeEscapeUTFString(TextParser.ExtractOne(line, """title"":""", """"))
			If Name.Length = 0 Then Name = "pixiv_" + Id.ToString()
			Dim url As String = TextParser.ExtractOne(line, """url"":""", """").Replace("\/", "/")
			ImageUrl = Pixiv.GenerateImageUrlFromCacheUrl(url)
		End Sub
		Shared Function CreateNewFromRank(section As String) As ImageInfo
			Dim info As New ImageInfo
			Dim tp As New TextParser(section)
			info.Id = tp.ExtractOne("data-id=""", """")
			info.Name = tp.ExtractOne("rel=""noopener"">", "</a>")
			info.ImageUrl = tp.ExtractOne("data-src=""", """")
			'Console.WriteLine(info.Name + "||" + info.Id + " || " + info.ImageUrl)
			Return info
		End Function
		Shared Function CreateNewFromRankJson(json As String) As ImageInfo
			Dim info As New ImageInfo
			Dim tp As New TextParser(json)
			info.Id = tp.ExtractOne("""illust_id"":", ",")
			info.Name = DeEscapeUTFString(tp.ExtractOne("""", """,""date"""))
			If info.Name.Length = 0 Then info.Name = "pixiv_" + info.Id.ToString()
			info.ImageUrl = GenerateImageUrlFromCacheUrl(tp.ExtractOne("""url"":""", """").Replace("\/", "/"))
			Return info
		End Function
		Shared Function DeEscapeUTFString(input As String) As String
			'Return Uri.UnescapeDataString(input)
			Dim reg As New Text.RegularExpressions.Regex("\\u([a-z]|[0-9]){4}")
			Dim code As Integer
			Dim sb As New Text.StringBuilder(input)
			For Each chItem As Text.RegularExpressions.Match In reg.Matches(input)
				code = CInt("&H" + chItem.Value.Replace("\u", ""))
				Try
					sb.Replace(chItem.Value, Char.ConvertFromUtf32(code))
				Catch ex As Exception
					sb.Replace(chItem.Value, "(" + chItem.Value.Replace("\u", "") + ")")
				End Try
			Next
			Return sb.ToString
		End Function
	End Structure
End Class

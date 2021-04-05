Imports System.Net

Public Class Pixiv

	' 用于操作的函数


	' 用于辅助实现的函数
	Protected Function GetJson(url As String) As String
		Return New WebProtocol(url, True).GetContentDocument()
	End Function
	Protected Overloads Sub DownloadImage(url As String, path As String)
		Dim wp As New WebProtocol(url)
		wp.Referer = "https://www.pixiv.net/"
		wp.Headers.Add("sec-fetch-site", "cross-site")
		'wp.Headers.Add("If-modified-since", since)
		Dim bytes As Byte() = wp.GetContentBytes()
		IO.File.WriteAllBytes(path + "1.jpg", bytes)
	End Sub
	Protected Overloads Function DownloadImage(iInfo As ImageInfo, path As String) As Integer
		Dim searchUrl As String = "https://www.pixiv.net/ajax/illust/{id}/pages?lang=zh"
		Dim pageJson = GetJson(searchUrl.Replace("{id}", iInfo.Id)) ' 谁能想一个页面里面多张图害得我还要改代码QwQ
		Dim i As Integer = 1
		For Each url As String In ExtractAllImageLinks(pageJson)
			Dim wp As New WebProtocol(url)
			wp.Referer = "https://www.pixiv.net/"
			wp.Headers.Add("sec-fetch-site", "cross-site")
			'wp.Headers.Add("If-modified-since", since)
			Dim bytes As Byte() = wp.GetContentBytes()
			Dim p As String = $"{path}[{iInfo.Id}]{iInfo.Name}_p{i}.jpg"
			For Each c As Char In IO.Path.GetInvalidPathChars()
				If p.Contains(c) Then p.Replace(iInfo.Name, "") : Exit For
				Next
				IO.File.WriteAllBytes(p, bytes)
				i += 1
			Next
			Return i - 1
	End Function
	Protected Function ExtractAllImageLinks(json As String) As String()
		Dim ret As New List(Of String)
		For Each item As String In TextParser.Extract(json.Replace("\/", "/"), """original"":""", """}")
			ret.Add(item)
		Next
		Return ret.ToArray
	End Function
	Protected Shared Function GenerateImageUrlFromCacheUrl(cacheUrl As String) As String
		Return "https://i.pximg.net/img-original/" + cacheUrl.Substring(cacheUrl.IndexOf("img/")).Replace("_square1200", "").Replace("_custom1200", "")
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
		Private Function DeEscapeUTFString(input As String) As String
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

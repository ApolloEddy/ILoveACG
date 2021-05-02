Public Class PixivRank : Inherits Pixiv
	Public Const RankLink As String = "https://www.pixiv.net/ranking.php"

	Public Sub DownloadDailyRankImages(path As String)
		Dim page = GetJson(RankLink)
		'Dim sections As List(Of String) = TextParser.Extract(page, "<div class="“_layout-thumbnail"">", "</h2>")
		Console.Write("正在采集数据...")
		Dim ImageInfoList As List(Of ImageInfo) = GetImageIngoList(10)
		Dim time = TextParser.ExtractOne(page, "<title>", "</title>").Split(" ").Last
		path += ImageInfo.DeEscapeUTFString(time).Split(" ")(0) + "\总榜\"
		IO.Directory.CreateDirectory(path)
		'For Each sec In sections
		'	ImageInfoList.Add(ImageInfo.CreateNewFromRank(sec))
		'Next
		Console.WriteLine($"正在将图片下载到目录 ""{path}""")
		DownloadImageList(ImageInfoList, path)
	End Sub

	Private Function GetImageIngoList(Optional range As Integer = 1) As List(Of ImageInfo)
		Dim ret As New List(Of ImageInfo)
		Dim templateLink = "https://www.pixiv.net/ranking.php?p={p}&format=json"
		Dim left = Console.CursorLeft
		Dim top = Console.CursorTop
		For i = 1 To range
			templateLink = templateLink.Replace("{p}", i.ToString)
			Dim json = GetJson(templateLink).Replace("{""contents"":", "")
			Dim dataBlock = TextParser.Extract(json, "{""title"":", "attr")
			For Each block In dataBlock
				ret.Add(ImageInfo.CreateNewFromRankJson(block))
			Next
			Dim process As String = $"  进度：{i}/{range} ==> 合 {Format(i * 100 / range, "00.00")}%"
			Console.SetCursorPosition(left, top)
			Console.Write(process)
			Console.Title = $"{My.Application.Info.Title}  正在采集图片数据：{Format(i * 100 / range, "00.00")}%"
		Next
		ret = ret.Distinct.ToList
		Console.WriteLine($"完毕，共采集[{range}]个页面，合[{ret.Count}]组图片！")
		Console.WriteLine()
		Console.Title = $"{My.Application.Info.Title}"
		Return ret
	End Function
End Class

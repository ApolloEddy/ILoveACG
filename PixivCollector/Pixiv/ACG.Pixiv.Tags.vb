''' <summary>
''' 
''' </summary>
Public Class PixivTags : Inherits Pixiv
	Public Sub New(tagName As String)
		Me.TagName = tagName
	End Sub
	Public ReadOnly Property TotalImages As Long
		Get
			Return _totalImages
		End Get
	End Property
	Private _totalImages As Long
	Public ReadOnly Property TagName As String

	Public Sub DownloadTagsImages(path As String)
		Dim json As String
		Dim imageInfoList As New List(Of ImageInfo)
		Dim Catched As Long
		' 第一步、迭代获取所有的图片信息
		Console.Write("正在获取图片信息...")
		Dim page As Integer = 1
		Do
			Try
				json = GetJson(TagUrl(TagName, page.ToString()))
				imageInfoList.AddRange(GetTagImageInfoList(json))
				page += 1
				'Threading.Thread.Sleep(Rand.Next(10, 30)) ' 随机数暂停，防止被反爬机制封杀
				'If page Mod 100 = 0 Then MsgBox($"imgs:{imageInfoList.Count}, total:{TotalImages}, page:{page}")
			Catch ex As Exception
				Exit Do
			End Try
		Loop
		imageInfoList = imageInfoList.Distinct().ToList() ' 列表去重
		Console.WriteLine("完成！")
		Console.WriteLine($"共计获取[{page}]页内容，[{imageInfoList.Count}]个文件信息，total值：{TotalImages}{vbNewLine}")

		' 第二步、下载获取的图片
		Catched = 0
		path += TagName + "\"
		Dim piccount As Integer
		IO.Directory.CreateDirectory(path)
		Console.WriteLine($"开始下载文件到目录：""{path}""")
		For Each img As ImageInfo In imageInfoList
			piccount = DownloadImage(img, path)
			Catched += piccount
			Console.WriteLine($"该作品有[{piccount}]张插图！ 进度: {imageInfoList.IndexOf(img) + 1}/{imageInfoList.Count}=>{Format(100 * (imageInfoList.IndexOf(img) + 1) / imageInfoList.Count, "0.00")}%")
		Next

		Console.WriteLine()
		Console.WriteLine($"下载完毕！共下载[{Catched}]张图片，有[{FailToDownloaded.Count}]张图片下载失败！")
	End Sub

	''' <summary>
	''' 根据json分析图片信息
	''' </summary>
	''' <param name="json">输入的json</param>
	''' <returns><see cref="List(Of ImageInfo)"/></returns>
	Private Function GetTagImageInfoList(json As String) As List(Of ImageInfo)
		Dim ret As New List(Of ImageInfo)
		Dim illust As String = TextParser.ExtractOne(json, """illustManga"":", """recent""") ' 捕获区块
		_totalImages = CLng(TextParser.ExtractOne(json, """total"":", ",")) ' 获取总数
		illust = TextParser.ExtractOne(illust, """data"":\[", "popular") ' 在拆分
		Dim iifo As ImageInfo
		For Each item As String In TextParser.Extract(illust, "{", "}") ' 拆分对象数组
			If Not (item.Contains("id") And item.Contains("title")) Then Continue For ' 捕获信息
			iifo = New ImageInfo(item)
			ret.Add(iifo)
			'Console.WriteLine(ret.Count.ToString + ".	" + iifo.Id + ":" + iifo.Name + ":" + iifo.ImageUrl)
		Next
		'Console.WriteLine(TotalImages)
		Return ret
	End Function
	''' <summary>
	''' 获取转义后的Tag（标签分区）链接
	''' </summary>
	''' <param name="key">待转义的字符串</param>
	''' <param name="p">页数</param>
	''' <returns><see cref="String"/></returns>
	Private Shared Function TagUrl(key As String, p As String) As String
		'https://www.pixiv.net/ajax/search/top/{key}?lang=zh
		Dim url As String = "https://www.pixiv.net/ajax/search/artworks/{key}?word={key}&order=date_d&mode=all&p={p}&s_mode=s_tag_full&type=all&lang=zh"
		Return url.Replace("{key}", Uri.EscapeUriString(key)).Replace("{p}", p)
	End Function
End Class


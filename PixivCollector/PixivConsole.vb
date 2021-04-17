Module PixivConsole

	Sub Main(arg As String())
		Test(arg(0))
		Console.ReadKey()
	End Sub
	Sub Test(tag As String)
		Dim p As New PixivTags(tag)
		p.DownloadTagsImages("F:\PixivImages\")
	End Sub
End Module

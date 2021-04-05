Module PixivConsole

	Sub Main()
		Test()
		Console.ReadKey()
	End Sub
	Sub Test()
		Dim p As New PixivTags("2233")
		p.DownloadTagsImages("F:\PixivImages\")
	End Sub
End Module

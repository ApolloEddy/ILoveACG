Module PixivConsole

	Sub Main()
		Test()
		Console.ReadKey()
	End Sub
	Sub Test()
		Dim p As New Pixiv()
		p.DownloadTagsImages("原神", "C:\Users\Administrator\Desktop\")
	End Sub
End Module

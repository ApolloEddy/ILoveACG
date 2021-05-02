Module PixivConsole

	Sub Main(arg As String())
		Console.Title = My.Application.Info.Title
		Dim stopwatch As New Stopwatch()
		stopwatch.Start()
		If arg.Length = 0 Then
			Dim p As New PixivRank()

		End If
		Test(arg(0))

		Console.WriteLine($"finish!  用时：{Format(stopwatch.ElapsedMilliseconds / 1000, "0.000")}s")
		Console.Title = My.Application.Info.Title + “完成”
		stopwatch.Stop()
		Console.ReadKey()
	End Sub
	Sub Test(tag As String)
		Dim p As New PixivTags(tag)
		p.DownloadTagsImages("F:\PixivImages\")
	End Sub
End Module

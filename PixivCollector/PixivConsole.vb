Module PixivConsole

	Sub Main(arg As String())

		' 以下为另一个测试单元
		Console.Title = My.Application.Info.Title
		Dim stopwatch As New Stopwatch()
		stopwatch.Start()
		If arg.Length = 0 Then
			Dim p As New PixivRank()

		End If

		' 以下为测试单元
		Test(arg(0))

		Console.WriteLine($"finish!  用时：{Format(stopwatch.ElapsedMilliseconds / 1000, "0.000")}s")

		' 以下为结束时控制代码
		Console.Title = My.Application.Info.Title + “完成”
		stopwatch.Stop()
		Console.ReadKey()
	End Sub
	Sub Test(tag As String)
		Dim p As New PixivTags(tag)
		p.DownloadTagsImages("F:\PixivImages\")
	End Sub
End Module

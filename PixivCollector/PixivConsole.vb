Module PixivConsole

	Sub Main(arg As String())
		Dim stopwatch As New Stopwatch()
		Stopwatch.StartNew()
		If arg.Length = 0 Then
			Dim p As New PixivRank()

		End If
		Test(arg(0))

		Console.WriteLine($"finish!  用时：{Format(stopwatch.ElapsedMilliseconds / 1000, "0.000")}s")
		stopwatch.Stop()
		Console.ReadKey()
	End Sub
	Sub Test(tag As String)
		Dim p As New PixivTags(tag)
		p.DownloadTagsImages("F:\PixivImages\")
	End Sub
End Module

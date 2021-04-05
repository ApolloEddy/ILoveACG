Imports System.IO

Public Class ImageInfoListDatabase
	Public ReadOnly Property ContactDBFile As String
	Public ReadOnly Property ImageInfoList As List(Of Pixiv.ImageInfo)
		Get
			Return _imageInfoList
		End Get
	End Property
	Private _imageInfoList As New List(Of Pixiv.ImageInfo)
	Private DataWriter As StreamWriter
	Private SourceText As String
	Public Sub New(dbPath As String)
		CheckPath(dbPath)
		ContactDBFile = dbPath
		LoadData()
	End Sub
	Private Sub LoadData()
		If Not File.Exists(ContactDBFile) Then
			DataWriter = File.CreateText(ContactDBFile)
		Else
			DataWriter = New StreamWriter(ContactDBFile)
		End If
		SourceText = File.OpenText(ContactDBFile).ReadToEnd()
		Dim tp As New TextParser(SourceText)
		Dim iifo As Pixiv.ImageInfo
		'<img><id>[id]</id><title>[title]</title><url>[url]</url></img>
		For Each line As String In tp.Extract("<img>", "</img>")
			iifo = New Pixiv.ImageInfo()
			iifo.Id = TextParser.ExtractOne(line, "<id>", "</id>")
			iifo.Name = TextParser.ExtractOne(line, "<title>", "</title>")
			iifo.ImageUrl = TextParser.ExtractOne(line, "<url>", "</url>")
			ImageInfoList.Add(iifo)
		Next
	End Sub
	Public Sub AddData(imgInfo As Pixiv.ImageInfo)
		Dim temp As String = My.Resources.Template.PixivImage
		temp.Replace("[id]", imgInfo.Id)
		temp.Replace("[title]", imgInfo.Name)
		temp.Replace("[url]", imgInfo.ImageUrl)
		DataWriter.WriteLine(temp)
		_imageInfoList.Add(imgInfo)
	End Sub

	Private Sub CheckPath(dbPath As String)
		If Not LCase(dbPath).EndsWith(".acgdb") Then Throw New FileLoadException("错误的文件后缀名！")
		'If Not File.Exists(dbPath) Then Throw New FileLoadException($"文件 ""{dbPath}"" 不存在！")
	End Sub

	Protected Overrides Sub Finalize()
		DataWriter.Close()
		DataWriter.Dispose()
		MyBase.Finalize()
	End Sub
	Public Overloads Sub Dispos()
		Finalize()
	End Sub
End Class

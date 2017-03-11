Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web

Public NotInheritable Class HttpTweak

    Private Const IfNoneMatch As String = "If-None-Match"
    Private Const Etag As String = "Etag"
    Private Const IfModifiedSince As String = "If-Modified-Since"
    Private Shared _dateTime As DateTime

    Public Shared Sub PublicCache(context As HttpContext)
        _dateTime = File.GetLastWriteTime(context.Request.PhysicalPath)
        SetConditionalGetHeaders(context, context.Request.Url.AbsoluteUri)
    End Sub

    Public Shared Sub PrivateCache(context As HttpContext, uri As String)
        SetConditionalGetHeaders(context, uri)
    End Sub

    Private Shared Sub SetConditionalGetHeaders(httpContext As HttpContext, url As String)
        SetConditionalGetHeaders(Hash(url), httpContext)
        SetConditionalGetHeaders(_dateTime, httpContext)
    End Sub

    Private Shared Sub SetConditionalGetHeaders(etag As String, context As HttpContext)
        Dim ifNoneMatchHeader As String = context.Request.Headers(IfNoneMatch)
        etag = """{etag}"""

        If ifNoneMatchHeader IsNot Nothing AndAlso ifNoneMatchHeader.Contains(",") Then
            ifNoneMatchHeader = ifNoneMatchHeader.Substring(0, ifNoneMatchHeader.IndexOf(",", StringComparison.Ordinal))
        End If

        context.Response.AppendHeader(HttpTweak.Etag, etag)
        context.Response.Cache.VaryByHeaders(IfNoneMatch) = True

        If etag = ifNoneMatchHeader Then
            context.Response.ClearContent()
            context.Response.StatusCode = CInt(HttpStatusCode.NotModified)
            context.Response.SuppressContent = True
        End If
    End Sub

    Private Shared Sub SetConditionalGetHeaders(lastModified As DateTime, context As HttpContext)
        Dim response As HttpResponse = context.Response
        Dim request As HttpRequest = context.Request
        lastModified = New DateTime(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second)

        Dim incomingDate As String = request.Headers(IfModifiedSince)

        response.Cache.SetLastModified(lastModified)

        Dim testDate As DateTime

        If DateTime.TryParse(incomingDate, testDate) AndAlso testDate = lastModified Then
            response.ClearContent()
            response.StatusCode = CInt(HttpStatusCode.NotModified)
            response.SuppressContent = True
        End If
    End Sub

    Private Shared Function Hash(value As String) As String
        Dim md5 As MD5 = New MD5Cng()

        Dim hashValue As Byte() = md5.ComputeHash(Encoding.UTF8.GetBytes(value))

        Dim stringBuilder = New StringBuilder()

        For index = 0 To hashValue.Length - 2
            stringBuilder.Append(hashValue(index).ToString("x2"))
        Next

        Return stringBuilder.ToString()
    End Function
End Class
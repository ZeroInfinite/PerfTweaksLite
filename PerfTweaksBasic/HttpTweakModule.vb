Imports System.IO
Imports System.Web

Module HttpTweakModule

    <System.Runtime.CompilerServices.Extension>
    Public Sub PublicCacheExtension(context As HttpContext)
        HttpTweak.PublicCache(context)
    End Sub

    <System.Runtime.CompilerServices.Extension>
    Public Sub PrivateCacheExtension(context As HttpContext, uri As String)
        HttpTweak.PrivateCache(context, uri)
    End Sub

End Module

/*
The MIT License (MIT)

Copyright (c) 2016-2017 Elastic Email, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Specialized;
using System.Threading.Tasks;


namespace ElasticEmailClient
{
    #region Utilities
    public class ApiResponse<T>
    {
        public bool success = false;
        public string error = null;
        public T Data
        {
            get;
            set;
        }
    }

    public class VoidApiResponse
    {
    }
    #region MIME HELPER
    internal class MimeMapping
    {
        private static Dictionary<string, string> mimeMappings = null;
        private static object locker = new object();
        private static readonly char[] _pathSeparatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar }; // from Path.GetFileName()

        public static string GetMimeMapping(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            EnsureMapping();
            fileName = GetFileName(fileName); // strip off path separators

            // some MIME types have complex extensions (like ".exe.config"), so we need to work left-to-right
            for (int i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '.')
                {
                    // potential extension - consult dictionary
                    string mimeType;
                    if (mimeMappings.TryGetValue(fileName.Substring(i), out mimeType))
                        return mimeType;
                }
            }

            // If we reached this point, either we couldn't find an extension, or the extension we found
            // wasn't recognized. In either case, the ".*" mapping is guaranteed to exist as a fallback.
            return mimeMappings[".*"];
        }

        private static string GetFileName(string path)
        {
            int pathSeparatorIndex = path.LastIndexOfAny(_pathSeparatorChars);
            return (pathSeparatorIndex >= 0) ? path.Substring(pathSeparatorIndex) : path;
        }

        private static void EnsureMapping()
        {
            // Ensure initialized only once
            if (mimeMappings == null)
            {
                lock (locker)
                {
                    if (mimeMappings == null)
                    {
                        PopulateMappings();
                    }
                }
            }
        }

        private static void PopulateMappings()
        {
            mimeMappings = new Dictionary<string, string>()
                    {
                    { ".*", "application/octet-stream" },
                    {".323", "text/h323"},
                    {".aaf", "application/octet-stream"},
                    {".aca", "application/octet-stream"},
                    {".accdb", "application/msaccess"},
                    {".accde", "application/msaccess"},
                    {".accdt", "application/msaccess"},
                    {".acx", "application/internet-property-stream"},
                    {".afm", "application/octet-stream"},
                    {".ai", "application/postscript"},
                    {".aif", "audio/x-aiff"},
                    {".aifc", "audio/aiff"},
                    {".aiff", "audio/aiff"},
                    {".application", "application/x-ms-application"},
                    {".art", "image/x-jg"},
                    {".asd", "application/octet-stream"},
                    {".asf", "video/x-ms-asf"},
                    {".asi", "application/octet-stream"},
                    {".asm", "text/plain"},
                    {".asr", "video/x-ms-asf"},
                    {".asx", "video/x-ms-asf"},
                    {".atom", "application/atom+xml"},
                    {".au", "audio/basic"},
                    {".avi", "video/x-msvideo"},
                    {".axs", "application/olescript"},
                    {".bas", "text/plain"},
                    {".bcpio", "application/x-bcpio"},
                    {".bin", "application/octet-stream"},
                    {".bmp", "image/bmp"},
                    {".c", "text/plain"},
                    {".cab", "application/octet-stream"},
                    {".calx", "application/vnd.ms-office.calx"},
                    {".cat", "application/vnd.ms-pki.seccat"},
                    {".cdf", "application/x-cdf"},
                    {".chm", "application/octet-stream"},
                    {".class", "application/x-java-applet"},
                    {".clp", "application/x-msclip"},
                    {".cmx", "image/x-cmx"},
                    {".cnf", "text/plain"},
                    {".cod", "image/cis-cod"},
                    {".cpio", "application/x-cpio"},
                    {".cpp", "text/plain"},
                    {".crd", "application/x-mscardfile"},
                    {".crl", "application/pkix-crl"},
                    {".crt", "application/x-x509-ca-cert"},
                    {".csh", "application/x-csh"},
                    {".css", "text/css"},
                    {".csv", "application/octet-stream"},
                    {".cur", "application/octet-stream"},
                    {".dcr", "application/x-director"},
                    {".deploy", "application/octet-stream"},
                    {".der", "application/x-x509-ca-cert"},
                    {".dib", "image/bmp"},
                    {".dir", "application/x-director"},
                    {".disco", "text/xml"},
                    {".dll", "application/x-msdownload"},
                    {".dll.config", "text/xml"},
                    {".dlm", "text/dlm"},
                    {".doc", "application/msword"},
                    {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
                    {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                    {".dot", "application/msword"},
                    {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
                    {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
                    {".dsp", "application/octet-stream"},
                    {".dtd", "text/xml"},
                    {".dvi", "application/x-dvi"},
                    {".dwf", "drawing/x-dwf"},
                    {".dwp", "application/octet-stream"},
                    {".dxr", "application/x-director"},
                    {".eml", "message/rfc822"},
                    {".emz", "application/octet-stream"},
                    {".eot", "application/octet-stream"},
                    {".eps", "application/postscript"},
                    {".etx", "text/x-setext"},
                    {".evy", "application/envoy"},
                    {".exe", "application/octet-stream"},
                    {".exe.config", "text/xml"},
                    {".fdf", "application/vnd.fdf"},
                    {".fif", "application/fractals"},
                    {".fla", "application/octet-stream"},
                    {".flr", "x-world/x-vrml"},
                    {".flv", "video/x-flv"},
                    {".gif", "image/gif"},
                    {".gtar", "application/x-gtar"},
                    {".gz", "application/x-gzip"},
                    {".h", "text/plain"},
                    {".hdf", "application/x-hdf"},
                    {".hdml", "text/x-hdml"},
                    {".hhc", "application/x-oleobject"},
                    {".hhk", "application/octet-stream"},
                    {".hhp", "application/octet-stream"},
                    {".hlp", "application/winhlp"},
                    {".hqx", "application/mac-binhex40"},
                    {".hta", "application/hta"},
                    {".htc", "text/x-component"},
                    {".htm", "text/html"},
                    {".html", "text/html"},
                    {".htt", "text/webviewhtml"},
                    {".hxt", "text/html"},
                    {".ico", "image/x-icon"},
                    {".ics", "application/octet-stream"},
                    {".ief", "image/ief"},
                    {".iii", "application/x-iphone"},
                    {".inf", "application/octet-stream"},
                    {".ins", "application/x-internet-signup"},
                    {".isp", "application/x-internet-signup"},
                    {".IVF", "video/x-ivf"},
                    {".jar", "application/java-archive"},
                    {".java", "application/octet-stream"},
                    {".jck", "application/liquidmotion"},
                    {".jcz", "application/liquidmotion"},
                    {".jfif", "image/pjpeg"},
                    {".jpb", "application/octet-stream"},
                    {".jpe", "image/jpeg"},
                    {".jpeg", "image/jpeg"},
                    {".jpg", "image/jpeg"},
                    {".js", "application/x-javascript"},
                    {".jsx", "text/jscript"},
                    {".latex", "application/x-latex"},
                    {".lit", "application/x-ms-reader"},
                    {".lpk", "application/octet-stream"},
                    {".lsf", "video/x-la-asf"},
                    {".lsx", "video/x-la-asf"},
                    {".lzh", "application/octet-stream"},
                    {".m13", "application/x-msmediaview"},
                    {".m14", "application/x-msmediaview"},
                    {".m1v", "video/mpeg"},
                    {".m3u", "audio/x-mpegurl"},
                    {".man", "application/x-troff-man"},
                    {".manifest", "application/x-ms-manifest"},
                    {".map", "text/plain"},
                    {".mdb", "application/x-msaccess"},
                    {".mdp", "application/octet-stream"},
                    {".me", "application/x-troff-me"},
                    {".mht", "message/rfc822"},
                    {".mhtml", "message/rfc822"},
                    {".mid", "audio/mid"},
                    {".midi", "audio/mid"},
                    {".mix", "application/octet-stream"},
                    {".mmf", "application/x-smaf"},
                    {".mno", "text/xml"},
                    {".mny", "application/x-msmoney"},
                    {".mov", "video/quicktime"},
                    {".movie", "video/x-sgi-movie"},
                    {".mp2", "video/mpeg"},
                    {".mp3", "audio/mpeg"},
                    {".mpa", "video/mpeg"},
                    {".mpe", "video/mpeg"},
                    {".mpeg", "video/mpeg"},
                    {".mpg", "video/mpeg"},
                    {".mpp", "application/vnd.ms-project"},
                    {".mpv2", "video/mpeg"},
                    {".ms", "application/x-troff-ms"},
                    {".msi", "application/octet-stream"},
                    {".mso", "application/octet-stream"},
                    {".mvb", "application/x-msmediaview"},
                    {".mvc", "application/x-miva-compiled"},
                    {".nc", "application/x-netcdf"},
                    {".nsc", "video/x-ms-asf"},
                    {".nws", "message/rfc822"},
                    {".ocx", "application/octet-stream"},
                    {".oda", "application/oda"},
                    {".odc", "text/x-ms-odc"},
                    {".ods", "application/oleobject"},
                    {".one", "application/onenote"},
                    {".onea", "application/onenote"},
                    {".onetoc", "application/onenote"},
                    {".onetoc2", "application/onenote"},
                    {".onetmp", "application/onenote"},
                    {".onepkg", "application/onenote"},
                    {".osdx", "application/opensearchdescription+xml"},
                    {".p10", "application/pkcs10"},
                    {".p12", "application/x-pkcs12"},
                    {".p7b", "application/x-pkcs7-certificates"},
                    {".p7c", "application/pkcs7-mime"},
                    {".p7m", "application/pkcs7-mime"},
                    {".p7r", "application/x-pkcs7-certreqresp"},
                    {".p7s", "application/pkcs7-signature"},
                    {".pbm", "image/x-portable-bitmap"},
                    {".pcx", "application/octet-stream"},
                    {".pcz", "application/octet-stream"},
                    {".pdf", "application/pdf"},
                    {".pfb", "application/octet-stream"},
                    {".pfm", "application/octet-stream"},
                    {".pfx", "application/x-pkcs12"},
                    {".pgm", "image/x-portable-graymap"},
                    {".pko", "application/vnd.ms-pki.pko"},
                    {".pma", "application/x-perfmon"},
                    {".pmc", "application/x-perfmon"},
                    {".pml", "application/x-perfmon"},
                    {".pmr", "application/x-perfmon"},
                    {".pmw", "application/x-perfmon"},
                    {".png", "image/png"},
                    {".pnm", "image/x-portable-anymap"},
                    {".pnz", "image/png"},
                    {".pot", "application/vnd.ms-powerpoint"},
                    {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
                    {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
                    {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
                    {".ppm", "image/x-portable-pixmap"},
                    {".pps", "application/vnd.ms-powerpoint"},
                    {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
                    {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
                    {".ppt", "application/vnd.ms-powerpoint"},
                    {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
                    {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                    {".prf", "application/pics-rules"},
                    {".prm", "application/octet-stream"},
                    {".prx", "application/octet-stream"},
                    {".ps", "application/postscript"},
                    {".psd", "application/octet-stream"},
                    {".psm", "application/octet-stream"},
                    {".psp", "application/octet-stream"},
                    {".pub", "application/x-mspublisher"},
                    {".qt", "video/quicktime"},
                    {".qtl", "application/x-quicktimeplayer"},
                    {".qxd", "application/octet-stream"},
                    {".ra", "audio/x-pn-realaudio"},
                    {".ram", "audio/x-pn-realaudio"},
                    {".rar", "application/octet-stream"},
                    {".ras", "image/x-cmu-raster"},
                    {".rf", "image/vnd.rn-realflash"},
                    {".rgb", "image/x-rgb"},
                    {".rm", "application/vnd.rn-realmedia"},
                    {".rmi", "audio/mid"},
                    {".roff", "application/x-troff"},
                    {".rpm", "audio/x-pn-realaudio-plugin"},
                    {".rtf", "application/rtf"},
                    {".rtx", "text/richtext"},
                    {".scd", "application/x-msschedule"},
                    {".sct", "text/scriptlet"},
                    {".sea", "application/octet-stream"},
                    {".setpay", "application/set-payment-initiation"},
                    {".setreg", "application/set-registration-initiation"},
                    {".sgml", "text/sgml"},
                    {".sh", "application/x-sh"},
                    {".shar", "application/x-shar"},
                    {".sit", "application/x-stuffit"},
                    {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
                    {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
                    {".smd", "audio/x-smd"},
                    {".smi", "application/octet-stream"},
                    {".smx", "audio/x-smd"},
                    {".smz", "audio/x-smd"},
                    {".snd", "audio/basic"},
                    {".snp", "application/octet-stream"},
                    {".spc", "application/x-pkcs7-certificates"},
                    {".spl", "application/futuresplash"},
                    {".src", "application/x-wais-source"},
                    {".ssm", "application/streamingmedia"},
                    {".sst", "application/vnd.ms-pki.certstore"},
                    {".stl", "application/vnd.ms-pki.stl"},
                    {".sv4cpio", "application/x-sv4cpio"},
                    {".sv4crc", "application/x-sv4crc"},
                    {".swf", "application/x-shockwave-flash"},
                    {".t", "application/x-troff"},
                    {".tar", "application/x-tar"},
                    {".tcl", "application/x-tcl"},
                    {".tex", "application/x-tex"},
                    {".texi", "application/x-texinfo"},
                    {".texinfo", "application/x-texinfo"},
                    {".tgz", "application/x-compressed"},
                    {".thmx", "application/vnd.ms-officetheme"},
                    {".thn", "application/octet-stream"},
                    {".tif", "image/tiff"},
                    {".tiff", "image/tiff"},
                    {".toc", "application/octet-stream"},
                    {".tr", "application/x-troff"},
                    {".trm", "application/x-msterminal"},
                    {".tsv", "text/tab-separated-values"},
                    {".ttf", "application/octet-stream"},
                    {".txt", "text/plain"},
                    {".u32", "application/octet-stream"},
                    {".uls", "text/iuls"},
                    {".ustar", "application/x-ustar"},
                    {".vbs", "text/vbscript"},
                    {".vcf", "text/x-vcard"},
                    {".vcs", "text/plain"},
                    {".vdx", "application/vnd.ms-visio.viewer"},
                    {".vml", "text/xml"},
                    {".vsd", "application/vnd.visio"},
                    {".vss", "application/vnd.visio"},
                    {".vst", "application/vnd.visio"},
                    {".vsto", "application/x-ms-vsto"},
                    {".vsw", "application/vnd.visio"},
                    {".vsx", "application/vnd.visio"},
                    {".vtx", "application/vnd.visio"},
                    {".wav", "audio/wav"},
                    {".wax", "audio/x-ms-wax"},
                    {".wbmp", "image/vnd.wap.wbmp"},
                    {".wcm", "application/vnd.ms-works"},
                    {".wdb", "application/vnd.ms-works"},
                    {".wks", "application/vnd.ms-works"},
                    {".wm", "video/x-ms-wm"},
                    {".wma", "audio/x-ms-wma"},
                    {".wmd", "application/x-ms-wmd"},
                    {".wmf", "application/x-msmetafile"},
                    {".wml", "text/vnd.wap.wml"},
                    {".wmlc", "application/vnd.wap.wmlc"},
                    {".wmls", "text/vnd.wap.wmlscript"},
                    {".wmlsc", "application/vnd.wap.wmlscriptc"},
                    {".wmp", "video/x-ms-wmp"},
                    {".wmv", "video/x-ms-wmv"},
                    {".wmx", "video/x-ms-wmx"},
                    {".wmz", "application/x-ms-wmz"},
                    {".wps", "application/vnd.ms-works"},
                    {".wri", "application/x-mswrite"},
                    {".wrl", "x-world/x-vrml"},
                    {".wrz", "x-world/x-vrml"},
                    {".wsdl", "text/xml"},
                    {".wvx", "video/x-ms-wvx"},
                    {".x", "application/directx"},
                    {".xaf", "x-world/x-vrml"},
                    {".xaml", "application/xaml+xml"},
                    {".xap", "application/x-silverlight-app"},
                    {".xbap", "application/x-ms-xbap"},
                    {".xbm", "image/x-xbitmap"},
                    {".xdr", "text/plain"},
                    {".xla", "application/vnd.ms-excel"},
                    {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
                    {".xlc", "application/vnd.ms-excel"},
                    {".xlm", "application/vnd.ms-excel"},
                    {".xls", "application/vnd.ms-excel"},
                    {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
                    {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
                    {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                    {".xlt", "application/vnd.ms-excel"},
                    {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
                    {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
                    {".xlw", "application/vnd.ms-excel"},
                    {".xml", "text/xml"},
                    {".xof", "x-world/x-vrml"},
                    {".xpm", "image/x-xpixmap"},
                    {".xps", "application/vnd.ms-xpsdocument"},
                    {".xsd", "text/xml"},
                    {".xsf", "text/xml"},
                    {".xsl", "text/xml"},
                    {".xslt", "text/xml"},
                    {".xsn", "application/octet-stream"},
                    {".xtp", "application/octet-stream"},
                    {".xwd", "image/x-xwindowdump"},
                    {".z", "application/x-compress"},
                    {".zip", "application/x-zip-compressed"},
                };
        }
    }
    #endregion MIME HELPER



    public static class ApiUtilities
    {
        public static async Task<ApiResponse<T>> PostAsync<T>(string requestAddress, Dictionary<string, string> values)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var postContent = new FormUrlEncodedContent(values))
                {
                    using (HttpResponseMessage response = await client.PostAsync(Api.ApiUri + requestAddress, postContent))
                    {
                        await response.EnsureSuccessStatusCodeAsync();
                        using (HttpContent content = response.Content)
                        {
                            string result = await content.ReadAsStringAsync();
                            ApiResponse<T> apiResponse = ApiResponseValidator.Validate<T>(result);
                            return apiResponse;
                        }
                    }
                }
            }
        }

        public static async Task<byte[]> HttpPostFileAsync(string url, List<ApiTypes.FileData> fileData, Dictionary<string, string> parameters)
        {
            try
            {
                url = Api.ApiUri + url;
                string boundary = DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Method = "POST";
                wr.Credentials = CredentialCache.DefaultCredentials;

                Stream rs = await wr.GetRequestStreamAsync();

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (string key in parameters.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, parameters[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }

                if (fileData != null)
                {
                    foreach (var file in fileData)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string headerTemplate = "Content-Disposition: form-data; name=\"filefoobarname\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n";
                        string header = string.Format(headerTemplate, file.FileName, file.ContentType);
                        byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                        rs.Write(headerbytes, 0, headerbytes.Length);
                        rs.Write(file.Content, 0, file.Content.Length);
                    }
                }

                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);


                using (WebResponse wresp = await wr.GetResponseAsync())
                {
                    MemoryStream response = new MemoryStream();
                    wresp.GetResponseStream().CopyTo(response);
                    return response.ToArray();
                }
            }
            catch (WebException webError)
            {
                // Throw exception with actual error message from response
                throw new WebException(((HttpWebResponse)webError.Response).StatusDescription, webError, webError.Status, webError.Response);
            }
        }

        public static async Task<ApiTypes.FileData> HttpGetFileAsync(string url, Dictionary<string, string> parameters)
        {
            try
            {
                url = Api.ApiUri + url;
                string queryString = BuildQueryString(parameters);

                if (queryString.Length > 0) url += "?" + queryString.ToString();

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.Method = "GET";
                wr.Credentials = CredentialCache.DefaultCredentials;

                using (WebResponse wresp = await wr.GetResponseAsync())
                {
                    MemoryStream response = new MemoryStream();
                    wresp.GetResponseStream().CopyTo(response);
                    if (response.Length == 0) throw new FileNotFoundException();
                    string cds = wresp.Headers["Content-Disposition"];
                    if (cds == null)
                    {
                        // This is a special case for critical exceptions
                        ApiResponse<string> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<string>>(Encoding.UTF8.GetString(response.ToArray()));
                        if (!apiRet.success) throw new Exception(apiRet.error);
                        return null;
                    }
                    else
                    {
                        ApiTypes.FileData fileData = new ApiTypes.FileData();
                        fileData.Content = response.ToArray();
                        fileData.ContentType = wresp.ContentType;
                        fileData.FileName = GetFileNameFromContentDisposition(cds);
                        return fileData;
                    }
                }
            }
            catch (WebException webError)
            {
                // Throw exception with actual error message from response
                throw new WebException(((HttpWebResponse)webError.Response).StatusDescription, webError, webError.Status, webError.Response);
            }
        }

        public static async Task<byte[]> HttpPutFileAsync(string url, ApiTypes.FileData fileData, Dictionary<string, string> parameters)
        {
            try
            {
                url = Api.ApiUri + url;
                string queryString = BuildQueryString(parameters);

                if (queryString.Length > 0) url += "?" + queryString.ToString();

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.ContentType = fileData.ContentType ?? "application/octet-stream";
                wr.Method = "PUT";

                wr.Credentials = CredentialCache.DefaultCredentials;
                wr.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                wr.Headers["Content-Disposition"] = "attachment; filename=\"" + fileData.FileName + "\"; size=" + fileData.Content.Length;

                Stream rs = await wr.GetRequestStreamAsync();
                rs.Write(fileData.Content, 0, fileData.Content.Length);

                using (WebResponse wresp = await wr.GetResponseAsync())
                {
                    MemoryStream response = new MemoryStream();
                    wresp.GetResponseStream().CopyTo(response);
                    return response.ToArray();
                }
            }
            catch (WebException webError)
            {
                // Throw exception with actual error message from response
                throw new WebException(((HttpWebResponse)webError.Response).StatusDescription, webError, webError.Status, webError.Response);
            }
        }

        static string BuildQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return null;

            StringBuilder query = new StringBuilder();
            string amp = string.Empty;
            foreach (KeyValuePair<string, string> kvp in parameters)
            {
                query.Append(amp);
                query.Append(WebUtility.UrlEncode(kvp.Key));
                query.Append(" = ");
                query.Append(WebUtility.UrlEncode(kvp.Value));
                amp = "&";
            }

            return query.ToString();
        }

        static string GetFileNameFromContentDisposition(string contentDisposition)
        {
            string[] chunks = contentDisposition.Split(';');
            string searchPhrase = "filename=";
            foreach (string chunk in chunks)
            {
                int index = contentDisposition.IndexOf(searchPhrase);
                if (index > 0)
                {
                    return contentDisposition.Substring(index + searchPhrase.Length);
                }
            }
            return "";
        }
    }

    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.Content != null)
                response.Content.Dispose();
            throw new SimpleHttpResponseException(response.StatusCode, content);
        }
    }

    public class SimpleHttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
        {
            StatusCode = statusCode;
        }
    }

    public class ApiResponseValidator
    {
        public static ApiResponse<T> Validate<T>(string apiResponse)
        {
            ApiResponse<T> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<T>>(apiResponse);
            if (!apiRet.success)
            {
                throw new Exception(apiRet.error);
            }
            return apiRet;
        }
    }

    #endregion

    // API version 2.33.0

    public static class Api
    {
        public static string ApiKey = "00000000-0000-0000-0000-000000000000";
        public static readonly string ApiUri = "https://api.elasticemail.com/v2";


        #region AccessToken functions
        /// <summary>
        /// Managinf ApiKeys
        /// </summary>
        public static class AccessToken
        {
            /// <summary>
            /// Add new AccessToken
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="tokenName"></param>
            /// <param name="accessLevel"></param>
            /// <returns>string</returns>
            public static async Task<string> AddAsync(string tokenName, ApiTypes.AccessLevel accessLevel)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("tokenName", tokenName);
                values.Add("accessLevel", accessLevel.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/accesstoken/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Permanently delete AccessToken.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="tokenName"></param>
            public static async Task DeleteAsync(string tokenName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("tokenName", tokenName);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/accesstoken/delete", values);
            }

            /// <summary>
            /// Edit AccessToken.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="tokenName"></param>
            /// <param name="accessLevel"></param>
            /// <param name="tokenNameNew"></param>
            public static async Task EditAsync(string tokenName, ApiTypes.AccessLevel accessLevel, string tokenNameNew = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("tokenName", tokenName);
                values.Add("accessLevel", accessLevel.ToString());
                if (tokenNameNew != null) values.Add("tokenNameNew", tokenNameNew);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/accesstoken/edit", values);
            }

            /// <summary>
            /// Get AccessToken list.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.AccessToken)</returns>
            public static async Task<List<ApiTypes.AccessToken>> GetListAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.AccessToken>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.AccessToken>>("/accesstoken/getlist", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Account functions
        /// <summary>
        /// Methods for managing your account and subaccounts.
        /// </summary>
        public static class Account
        {
            /// <summary>
            /// Create new subaccount and provide most important data about it.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="password">Current password.</param>
            /// <param name="confirmPassword">Repeat new password.</param>
            /// <param name="requiresEmailCredits">True, if account needs credits to send emails. Otherwise, false</param>
            /// <param name="enableLitmusTest">True, if account is able to send template tests to Litmus. Otherwise, false</param>
            /// <param name="requiresLitmusCredits">True, if account needs credits to send emails. Otherwise, false</param>
            /// <param name="maxContacts">Maximum number of contacts the account can have</param>
            /// <param name="enablePrivateIPRequest">True, if account can request for private IP on its own. Otherwise, false</param>
            /// <param name="sendActivation">True, if you want to send activation email to this account. Otherwise, false</param>
            /// <param name="returnUrl">URL to navigate to after account creation</param>
            /// <param name="sendingPermission">Sending permission setting for account</param>
            /// <param name="enableContactFeatures">True, if you want to use Contact Delivery Tools.  Otherwise, false</param>
            /// <param name="poolName">Private IP required. Name of the custom IP Pool which Sub Account should use to send its emails. Leave empty for the default one or if no Private IPs have been bought</param>
            /// <param name="emailSizeLimit">Maximum size of email including attachments in MB's</param>
            /// <param name="dailySendLimit">Amount of emails account can send daily</param>
            /// <returns>string</returns>
            public static async Task<string> AddSubAccountAsync(string email, string password, string confirmPassword, bool requiresEmailCredits = false, bool enableLitmusTest = false, bool requiresLitmusCredits = false, int maxContacts = 0, bool enablePrivateIPRequest = true, bool sendActivation = false, string returnUrl = null, ApiTypes.SendingPermission? sendingPermission = null, bool? enableContactFeatures = null, string poolName = null, int emailSizeLimit = 10, int? dailySendLimit = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                values.Add("password", password);
                values.Add("confirmPassword", confirmPassword);
                if (requiresEmailCredits != false) values.Add("requiresEmailCredits", requiresEmailCredits.ToString());
                if (enableLitmusTest != false) values.Add("enableLitmusTest", enableLitmusTest.ToString());
                if (requiresLitmusCredits != false) values.Add("requiresLitmusCredits", requiresLitmusCredits.ToString());
                if (maxContacts != 0) values.Add("maxContacts", maxContacts.ToString());
                if (enablePrivateIPRequest != true) values.Add("enablePrivateIPRequest", enablePrivateIPRequest.ToString());
                if (sendActivation != false) values.Add("sendActivation", sendActivation.ToString());
                if (returnUrl != null) values.Add("returnUrl", returnUrl);
                if (sendingPermission != null) values.Add("sendingPermission", Newtonsoft.Json.JsonConvert.SerializeObject(sendingPermission));
                if (enableContactFeatures != null) values.Add("enableContactFeatures", enableContactFeatures.ToString());
                if (poolName != null) values.Add("poolName", poolName);
                if (emailSizeLimit != 10) values.Add("emailSizeLimit", emailSizeLimit.ToString());
                if (dailySendLimit != null) values.Add("dailySendLimit", dailySendLimit.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/account/addsubaccount", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Add email, template or litmus credits to a sub-account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="credits">Amount of credits to add</param>
            /// <param name="notes">Specific notes about the transaction</param>
            /// <param name="creditType">Type of credits to add (Email or Litmus)</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to add credits to. Use subAccountEmail or publicAccountID not both.</param>
            public static async Task AddSubAccountCreditsAsync(int credits, string notes, ApiTypes.CreditType creditType = ApiTypes.CreditType.Email, string subAccountEmail = null, string publicAccountID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("credits", credits.ToString());
                values.Add("notes", notes);
                if (creditType != ApiTypes.CreditType.Email) values.Add("creditType", creditType.ToString());
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/addsubaccountcredits", values);
            }

            /// <summary>
            /// Change your email address. Remember, that your email address is used as login!
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="newEmail">New email address.</param>
            /// <param name="confirmEmail">New email address.</param>
            /// <param name="sourceUrl">URL from which request was sent.</param>
            /// <returns>string</returns>
            public static async Task<string> ChangeEmailAsync(string newEmail, string confirmEmail, string sourceUrl = "https://elasticemail.com/account/")
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("newEmail", newEmail);
                values.Add("confirmEmail", confirmEmail);
                if (sourceUrl != "https://elasticemail.com/account/") values.Add("sourceUrl", sourceUrl);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/account/changeemail", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Create new password for your account. Password needs to be at least 6 characters long.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="currentPassword">Current password.</param>
            /// <param name="newPassword">New password for account.</param>
            /// <param name="confirmPassword">Repeat new password.</param>
            public static async Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("currentPassword", currentPassword);
                values.Add("newPassword", newPassword);
                values.Add("confirmPassword", confirmPassword);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/changepassword", values);
            }

            /// <summary>
            /// Deletes specified Subaccount
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="notify">True, if you want to send an email notification. Otherwise, false</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to delete. Use subAccountEmail or publicAccountID not both.</param>
            /// <param name="deleteDomains"></param>
            public static async Task DeleteSubAccountAsync(bool notify = true, string subAccountEmail = null, string publicAccountID = null, bool deleteDomains = true)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (notify != true) values.Add("notify", notify.ToString());
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                if (deleteDomains != true) values.Add("deleteDomains", deleteDomains.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/deletesubaccount", values);
            }

            /// <summary>
            /// Validate account's ability to send e-mail
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.AccountSendStatus</returns>
            public static async Task<ApiTypes.AccountSendStatus> GetAccountAbilityToSendEmailAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.AccountSendStatus> apiResponse = await ApiUtilities.PostAsync<ApiTypes.AccountSendStatus>("/account/getaccountabilitytosendemail", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns API Key for the given Sub Account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to retrieve sub-account API Key. Use subAccountEmail or publicAccountID not both.</param>
            /// <returns>string</returns>
            public static async Task<string> GetSubAccountApiKeyAsync(string subAccountEmail = null, string publicAccountID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/account/getsubaccountapikey", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists all of your subaccounts
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.SubAccount)</returns>
            public static async Task<List<ApiTypes.SubAccount>> GetSubAccountListAsync(int limit = 0, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.SubAccount>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.SubAccount>>("/account/getsubaccountlist", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Loads your account. Returns detailed information about your account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.Account</returns>
            public static async Task<ApiTypes.Account> LoadAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.Account> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Account>("/account/load", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Load advanced options of your account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.AdvancedOptions</returns>
            public static async Task<ApiTypes.AdvancedOptions> LoadAdvancedOptionsAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.AdvancedOptions> apiResponse = await ApiUtilities.PostAsync<ApiTypes.AdvancedOptions>("/account/loadadvancedoptions", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists email credits history
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.EmailCredits)</returns>
            public static async Task<List<ApiTypes.EmailCredits>> LoadEmailCreditsHistoryAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.EmailCredits>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.EmailCredits>>("/account/loademailcreditshistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Loads your account. Returns detailed information about your account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.Account</returns>
            public static async Task<ApiTypes.Account> LoadInfoAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.Account> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Account>("/account/loadinfo", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists litmus credits history
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.LitmusCredits)</returns>
            public static async Task<List<ApiTypes.LitmusCredits>> LoadLitmusCreditsHistoryAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.LitmusCredits>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.LitmusCredits>>("/account/loadlitmuscreditshistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows queue of newest notifications - very useful when you want to check what happened with mails that were not received.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.NotificationQueue)</returns>
            public static async Task<List<ApiTypes.NotificationQueue>> LoadNotificationQueueAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.NotificationQueue>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.NotificationQueue>>("/account/loadnotificationqueue", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists all payments
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="fromDate">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="toDate">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>List(ApiTypes.Payment)</returns>
            public static async Task<List<ApiTypes.Payment>> LoadPaymentHistoryAsync(int limit, int offset, DateTime fromDate, DateTime toDate)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("limit", limit.ToString());
                values.Add("offset", offset.ToString());
                values.Add("fromDate", fromDate.ToString("M/d/yyyy h:mm:ss tt"));
                values.Add("toDate", toDate.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<List<ApiTypes.Payment>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Payment>>("/account/loadpaymenthistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists all referral payout history
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.Payment)</returns>
            public static async Task<List<ApiTypes.Payment>> LoadPayoutHistoryAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.Payment>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Payment>>("/account/loadpayouthistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows information about your referral details
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.Referral</returns>
            public static async Task<ApiTypes.Referral> LoadReferralDetailsAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.Referral> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Referral>("/account/loadreferraldetails", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows latest changes in your sending reputation
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.ReputationHistory)</returns>
            public static async Task<List<ApiTypes.ReputationHistory>> LoadReputationHistoryAsync(int limit = 20, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (limit != 20) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.ReputationHistory>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.ReputationHistory>>("/account/loadreputationhistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows detailed information about your actual reputation score
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.ReputationDetail</returns>
            public static async Task<ApiTypes.ReputationDetail> LoadReputationImpactAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.ReputationDetail> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ReputationDetail>("/account/loadreputationimpact", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns detailed spam check.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.SpamCheck)</returns>
            public static async Task<List<ApiTypes.SpamCheck>> LoadSpamCheckAsync(int limit = 20, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (limit != 20) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.SpamCheck>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.SpamCheck>>("/account/loadspamcheck", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists email credits history for sub-account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to list history for. Use subAccountEmail or publicAccountID not both.</param>
            /// <returns>List(ApiTypes.EmailCredits)</returns>
            public static async Task<List<ApiTypes.EmailCredits>> LoadSubAccountsEmailCreditsHistoryAsync(string subAccountEmail = null, string publicAccountID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                ApiResponse<List<ApiTypes.EmailCredits>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.EmailCredits>>("/account/loadsubaccountsemailcreditshistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Loads settings of subaccount
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to load settings for. Use subAccountEmail or publicAccountID not both.</param>
            /// <returns>ApiTypes.SubAccountSettings</returns>
            public static async Task<ApiTypes.SubAccountSettings> LoadSubAccountSettingsAsync(string subAccountEmail = null, string publicAccountID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                ApiResponse<ApiTypes.SubAccountSettings> apiResponse = await ApiUtilities.PostAsync<ApiTypes.SubAccountSettings>("/account/loadsubaccountsettings", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists litmus credits history for sub-account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to list history for. Use subAccountEmail or publicAccountID not both.</param>
            /// <returns>List(ApiTypes.LitmusCredits)</returns>
            public static async Task<List<ApiTypes.LitmusCredits>> LoadSubAccountsLitmusCreditsHistoryAsync(string subAccountEmail = null, string publicAccountID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                ApiResponse<List<ApiTypes.LitmusCredits>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.LitmusCredits>>("/account/loadsubaccountslitmuscreditshistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows usage of your account in given time.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>List(ApiTypes.Usage)</returns>
            public static async Task<List<ApiTypes.Usage>> LoadUsageAsync(DateTime from, DateTime to)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("from", from.ToString("M/d/yyyy h:mm:ss tt"));
                values.Add("to", to.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<List<ApiTypes.Usage>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Usage>>("/account/loadusage", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows summary for your account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.AccountOverview</returns>
            public static async Task<ApiTypes.AccountOverview> OverviewAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.AccountOverview> apiResponse = await ApiUtilities.PostAsync<ApiTypes.AccountOverview>("/account/overview", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows you account's profile basic overview
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.Profile</returns>
            public static async Task<ApiTypes.Profile> ProfileOverviewAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.Profile> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Profile>("/account/profileoverview", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Remove email, template or litmus credits from a sub-account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="creditType">Type of credits to add (Email or Litmus)</param>
            /// <param name="notes">Specific notes about the transaction</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to remove credits from. Use subAccountEmail or publicAccountID not both.</param>
            /// <param name="credits">Amount of credits to remove</param>
            /// <param name="removeAll">Remove all credits of this type from sub-account (overrides credits if provided)</param>
            public static async Task RemoveSubAccountCreditsAsync(ApiTypes.CreditType creditType, string notes, string subAccountEmail = null, string publicAccountID = null, int? credits = null, bool removeAll = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("creditType", creditType.ToString());
                values.Add("notes", notes);
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                if (credits != null) values.Add("credits", credits.ToString());
                if (removeAll != false) values.Add("removeAll", removeAll.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/removesubaccountcredits", values);
            }

            /// <summary>
            /// Request a new default APIKey.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>string</returns>
            public static async Task<string> RequestNewApiKeyAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/account/requestnewapikey", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Request premium support for your account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            public static async Task RequestPremiumSupportAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/requestpremiumsupport", values);
            }

            /// <summary>
            /// Request a private IP for your Account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="count">Number of items.</param>
            /// <param name="notes">Free form field of notes</param>
            public static async Task RequestPrivateIPAsync(int count, string notes)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("count", count.ToString());
                values.Add("notes", notes);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/requestprivateip", values);
            }

            /// <summary>
            /// Update sending and tracking options of your account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="enableClickTracking">True, if you want to track clicks. Otherwise, false</param>
            /// <param name="enableLinkClickTracking">True, if you want to track by link tracking. Otherwise, false</param>
            /// <param name="manageSubscriptions">True, if you want to display your labels on your unsubscribe form. Otherwise, false</param>
            /// <param name="manageSubscribedOnly">True, if you want to only display labels that the contact is subscribed to on your unsubscribe form. Otherwise, false</param>
            /// <param name="transactionalOnUnsubscribe">True, if you want to display an option for the contact to opt into transactional email only on your unsubscribe form. Otherwise, false</param>
            /// <param name="skipListUnsubscribe">True, if you do not want to use list-unsubscribe headers. Otherwise, false</param>
            /// <param name="autoTextFromHtml">True, if text BODY of message should be created automatically. Otherwise, false</param>
            /// <param name="allowCustomHeaders">True, if you want to apply custom headers to your emails. Otherwise, false</param>
            /// <param name="bccEmail">Email address to send a copy of all email to.</param>
            /// <param name="contentTransferEncoding">Type of content encoding</param>
            /// <param name="emailNotificationForError">True, if you want bounce notifications returned. Otherwise, false</param>
            /// <param name="emailNotificationEmail">Specific email address to send bounce email notifications to.</param>
            /// <param name="webNotificationUrl">URL address to receive web notifications to parse and process.</param>
            /// <param name="webNotificationNotifyOncePerEmail">True, if you want to receive notifications for each type only once per email. Otherwise, false</param>
            /// <param name="webNotificationForSent">True, if you want to send web notifications for sent email. Otherwise, false</param>
            /// <param name="webNotificationForOpened">True, if you want to send web notifications for opened email. Otherwise, false</param>
            /// <param name="webNotificationForClicked">True, if you want to send web notifications for clicked email. Otherwise, false</param>
            /// <param name="webNotificationForUnsubscribed">True, if you want to send web notifications for unsubscribed email. Otherwise, false</param>
            /// <param name="webNotificationForAbuseReport">True, if you want to send web notifications for complaint email. Otherwise, false</param>
            /// <param name="webNotificationForError">True, if you want to send web notifications for bounced email. Otherwise, false</param>
            /// <param name="hubCallBackUrl">URL used for tracking action of inbound emails</param>
            /// <param name="inboundDomain">Domain you use as your inbound domain</param>
            /// <param name="inboundContactsOnly">True, if you want inbound email to only process contacts from your account. Otherwise, false</param>
            /// <param name="lowCreditNotification">True, if you want to receive low credit email notifications. Otherwise, false</param>
            /// <param name="enableUITooltips">True, if account has tooltips active. Otherwise, false</param>
            /// <param name="enableContactFeatures">True, if you want to use Contact Delivery Tools.  Otherwise, false</param>
            /// <param name="notificationsEmails">Email addresses to send a copy of all notifications from our system. Separated by semicolon</param>
            /// <param name="unsubscribeNotificationsEmails">Emails, separated by semicolon, to which the notification about contact unsubscribing should be sent to</param>
            /// <param name="logoUrl">URL to your logo image.</param>
            /// <param name="enableTemplateScripting">True, if you want to use template scripting in your emails {{}}. Otherwise, false</param>
            /// <param name="staleContactScore">(0 means this functionality is NOT enabled) Score, depending on the number of times you have sent to a recipient, at which the given recipient should be moved to the Stale status</param>
            /// <param name="staleContactInactiveDays">(0 means this functionality is NOT enabled) Number of days of inactivity for a contact after which the given recipient should be moved to the Stale status</param>
            /// <param name="deliveryReason">Why your clients are receiving your emails.</param>
            /// <param name="tutorialsEnabled">True, if you want to enable Dashboard Tutotials</param>
            /// <param name="enableOpenTracking">True, if you want to track opens. Otherwise, false</param>
            /// <param name="consentTrackingOnUnsubscribe"></param>
            /// <returns>ApiTypes.AdvancedOptions</returns>
            public static async Task<ApiTypes.AdvancedOptions> UpdateAdvancedOptionsAsync(bool? enableClickTracking = null, bool? enableLinkClickTracking = null, bool? manageSubscriptions = null, bool? manageSubscribedOnly = null, bool? transactionalOnUnsubscribe = null, bool? skipListUnsubscribe = null, bool? autoTextFromHtml = null, bool? allowCustomHeaders = null, string bccEmail = null, string contentTransferEncoding = null, bool? emailNotificationForError = null, string emailNotificationEmail = null, string webNotificationUrl = null, bool? webNotificationNotifyOncePerEmail = null, bool? webNotificationForSent = null, bool? webNotificationForOpened = null, bool? webNotificationForClicked = null, bool? webNotificationForUnsubscribed = null, bool? webNotificationForAbuseReport = null, bool? webNotificationForError = null, string hubCallBackUrl = "", string inboundDomain = null, bool? inboundContactsOnly = null, bool? lowCreditNotification = null, bool? enableUITooltips = null, bool? enableContactFeatures = null, string notificationsEmails = null, string unsubscribeNotificationsEmails = null, string logoUrl = null, bool? enableTemplateScripting = true, int? staleContactScore = null, int? staleContactInactiveDays = null, string deliveryReason = null, bool? tutorialsEnabled = null, bool? enableOpenTracking = null, bool? consentTrackingOnUnsubscribe = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (enableClickTracking != null) values.Add("enableClickTracking", enableClickTracking.ToString());
                if (enableLinkClickTracking != null) values.Add("enableLinkClickTracking", enableLinkClickTracking.ToString());
                if (manageSubscriptions != null) values.Add("manageSubscriptions", manageSubscriptions.ToString());
                if (manageSubscribedOnly != null) values.Add("manageSubscribedOnly", manageSubscribedOnly.ToString());
                if (transactionalOnUnsubscribe != null) values.Add("transactionalOnUnsubscribe", transactionalOnUnsubscribe.ToString());
                if (skipListUnsubscribe != null) values.Add("skipListUnsubscribe", skipListUnsubscribe.ToString());
                if (autoTextFromHtml != null) values.Add("autoTextFromHtml", autoTextFromHtml.ToString());
                if (allowCustomHeaders != null) values.Add("allowCustomHeaders", allowCustomHeaders.ToString());
                if (bccEmail != null) values.Add("bccEmail", bccEmail);
                if (contentTransferEncoding != null) values.Add("contentTransferEncoding", contentTransferEncoding);
                if (emailNotificationForError != null) values.Add("emailNotificationForError", emailNotificationForError.ToString());
                if (emailNotificationEmail != null) values.Add("emailNotificationEmail", emailNotificationEmail);
                if (webNotificationUrl != null) values.Add("webNotificationUrl", webNotificationUrl);
                if (webNotificationNotifyOncePerEmail != null) values.Add("webNotificationNotifyOncePerEmail", webNotificationNotifyOncePerEmail.ToString());
                if (webNotificationForSent != null) values.Add("webNotificationForSent", webNotificationForSent.ToString());
                if (webNotificationForOpened != null) values.Add("webNotificationForOpened", webNotificationForOpened.ToString());
                if (webNotificationForClicked != null) values.Add("webNotificationForClicked", webNotificationForClicked.ToString());
                if (webNotificationForUnsubscribed != null) values.Add("webNotificationForUnsubscribed", webNotificationForUnsubscribed.ToString());
                if (webNotificationForAbuseReport != null) values.Add("webNotificationForAbuseReport", webNotificationForAbuseReport.ToString());
                if (webNotificationForError != null) values.Add("webNotificationForError", webNotificationForError.ToString());
                if (hubCallBackUrl != "") values.Add("hubCallBackUrl", hubCallBackUrl);
                if (inboundDomain != null) values.Add("inboundDomain", inboundDomain);
                if (inboundContactsOnly != null) values.Add("inboundContactsOnly", inboundContactsOnly.ToString());
                if (lowCreditNotification != null) values.Add("lowCreditNotification", lowCreditNotification.ToString());
                if (enableUITooltips != null) values.Add("enableUITooltips", enableUITooltips.ToString());
                if (enableContactFeatures != null) values.Add("enableContactFeatures", enableContactFeatures.ToString());
                if (notificationsEmails != null) values.Add("notificationsEmails", notificationsEmails);
                if (unsubscribeNotificationsEmails != null) values.Add("unsubscribeNotificationsEmails", unsubscribeNotificationsEmails);
                if (logoUrl != null) values.Add("logoUrl", logoUrl);
                if (enableTemplateScripting != true) values.Add("enableTemplateScripting", enableTemplateScripting.ToString());
                if (staleContactScore != null) values.Add("staleContactScore", staleContactScore.ToString());
                if (staleContactInactiveDays != null) values.Add("staleContactInactiveDays", staleContactInactiveDays.ToString());
                if (deliveryReason != null) values.Add("deliveryReason", deliveryReason);
                if (tutorialsEnabled != null) values.Add("tutorialsEnabled", tutorialsEnabled.ToString());
                if (enableOpenTracking != null) values.Add("enableOpenTracking", enableOpenTracking.ToString());
                if (consentTrackingOnUnsubscribe != null) values.Add("consentTrackingOnUnsubscribe", consentTrackingOnUnsubscribe.ToString());
                ApiResponse<ApiTypes.AdvancedOptions> apiResponse = await ApiUtilities.PostAsync<ApiTypes.AdvancedOptions>("/account/updateadvancedoptions", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Update settings of your private branding. These settings are needed, if you want to use Elastic Email under your brand.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="enablePrivateBranding">True: Turn on or off ability to send mails under your brand. Otherwise, false</param>
            /// <param name="logoUrl">URL to your logo image.</param>
            /// <param name="supportLink">Address to your support.</param>
            /// <param name="privateBrandingUrl">Subdomain for your rebranded service</param>
            /// <param name="smtpAddress">Address of SMTP server.</param>
            /// <param name="smtpAlternative">Address of alternative SMTP server.</param>
            /// <param name="paymentUrl">URL for making payments.</param>
            public static async Task UpdateCustomBrandingAsync(bool enablePrivateBranding = false, string logoUrl = null, string supportLink = null, string privateBrandingUrl = null, string smtpAddress = null, string smtpAlternative = null, string paymentUrl = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (enablePrivateBranding != false) values.Add("enablePrivateBranding", enablePrivateBranding.ToString());
                if (logoUrl != null) values.Add("logoUrl", logoUrl);
                if (supportLink != null) values.Add("supportLink", supportLink);
                if (privateBrandingUrl != null) values.Add("privateBrandingUrl", privateBrandingUrl);
                if (smtpAddress != null) values.Add("smtpAddress", smtpAddress);
                if (smtpAlternative != null) values.Add("smtpAlternative", smtpAlternative);
                if (paymentUrl != null) values.Add("paymentUrl", paymentUrl);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/updatecustombranding", values);
            }

            /// <summary>
            /// Update http notification URL.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="url">URL of notification.</param>
            /// <param name="notifyOncePerEmail">True, if you want to receive notifications for each type only once per email. Otherwise, false</param>
            /// <param name="settings">Http notification settings serialized to JSON </param>
            public static async Task UpdateHttpNotificationAsync(string url, bool notifyOncePerEmail = false, string settings = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("url", url);
                if (notifyOncePerEmail != false) values.Add("notifyOncePerEmail", notifyOncePerEmail.ToString());
                if (settings != null) values.Add("settings", settings);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/updatehttpnotification", values);
            }

            /// <summary>
            /// Update your profile.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="firstName">First name.</param>
            /// <param name="lastName">Last name.</param>
            /// <param name="address1">First line of address.</param>
            /// <param name="city">City.</param>
            /// <param name="state">State or province.</param>
            /// <param name="zip">Zip/postal code.</param>
            /// <param name="countryID">Numeric ID of country. A file with the list of countries is available <a href="http://api.elasticemail.com/public/countries"><b>here</b></a></param>
            /// <param name="marketingConsent">True if you want to receive newsletters from Elastic Email. Otherwise, false. Empty to leave the current value.</param>
            /// <param name="address2">Second line of address.</param>
            /// <param name="company">Company name.</param>
            /// <param name="website">HTTP address of your website.</param>
            /// <param name="logoUrl">URL to your logo image.</param>
            /// <param name="taxCode">Code used for tax purposes.</param>
            /// <param name="phone">Phone number</param>
            public static async Task UpdateProfileAsync(string firstName, string lastName, string address1, string city, string state, string zip, int countryID, bool? marketingConsent = null, string address2 = null, string company = null, string website = null, string logoUrl = null, string taxCode = null, string phone = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("firstName", firstName);
                values.Add("lastName", lastName);
                values.Add("address1", address1);
                values.Add("city", city);
                values.Add("state", state);
                values.Add("zip", zip);
                values.Add("countryID", countryID.ToString());
                if (marketingConsent != null) values.Add("marketingConsent", marketingConsent.ToString());
                if (address2 != null) values.Add("address2", address2);
                if (company != null) values.Add("company", company);
                if (website != null) values.Add("website", website);
                if (logoUrl != null) values.Add("logoUrl", logoUrl);
                if (taxCode != null) values.Add("taxCode", taxCode);
                if (phone != null) values.Add("phone", phone);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/updateprofile", values);
            }

            /// <summary>
            /// Updates settings of specified subaccount
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="requiresEmailCredits">True, if account needs credits to send emails. Otherwise, false</param>
            /// <param name="monthlyRefillCredits">Amount of credits added to account automatically</param>
            /// <param name="requiresLitmusCredits">True, if account needs credits to send emails. Otherwise, false</param>
            /// <param name="enableLitmusTest">True, if account is able to send template tests to Litmus. Otherwise, false</param>
            /// <param name="dailySendLimit">Amount of emails account can send daily</param>
            /// <param name="emailSizeLimit">Maximum size of email including attachments in MB's</param>
            /// <param name="enablePrivateIPRequest">True, if account can request for private IP on its own. Otherwise, false</param>
            /// <param name="maxContacts">Maximum number of contacts the account can have</param>
            /// <param name="subAccountEmail">Email address of sub-account</param>
            /// <param name="publicAccountID">Public key of sub-account to update. Use subAccountEmail or publicAccountID not both.</param>
            /// <param name="sendingPermission">Sending permission setting for account</param>
            /// <param name="enableContactFeatures">True, if you want to use Contact Delivery Tools.  Otherwise, false</param>
            /// <param name="poolName">Name of your custom IP Pool to be used in the sending process</param>
            public static async Task UpdateSubAccountSettingsAsync(bool requiresEmailCredits = false, int monthlyRefillCredits = 0, bool requiresLitmusCredits = false, bool enableLitmusTest = false, int? dailySendLimit = null, int emailSizeLimit = 10, bool enablePrivateIPRequest = false, int maxContacts = 0, string subAccountEmail = null, string publicAccountID = null, ApiTypes.SendingPermission? sendingPermission = null, bool? enableContactFeatures = null, string poolName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (requiresEmailCredits != false) values.Add("requiresEmailCredits", requiresEmailCredits.ToString());
                if (monthlyRefillCredits != 0) values.Add("monthlyRefillCredits", monthlyRefillCredits.ToString());
                if (requiresLitmusCredits != false) values.Add("requiresLitmusCredits", requiresLitmusCredits.ToString());
                if (enableLitmusTest != false) values.Add("enableLitmusTest", enableLitmusTest.ToString());
                if (dailySendLimit != null) values.Add("dailySendLimit", dailySendLimit.ToString());
                if (emailSizeLimit != 10) values.Add("emailSizeLimit", emailSizeLimit.ToString());
                if (enablePrivateIPRequest != false) values.Add("enablePrivateIPRequest", enablePrivateIPRequest.ToString());
                if (maxContacts != 0) values.Add("maxContacts", maxContacts.ToString());
                if (subAccountEmail != null) values.Add("subAccountEmail", subAccountEmail);
                if (publicAccountID != null) values.Add("publicAccountID", publicAccountID);
                if (sendingPermission != null) values.Add("sendingPermission", Newtonsoft.Json.JsonConvert.SerializeObject(sendingPermission));
                if (enableContactFeatures != null) values.Add("enableContactFeatures", enableContactFeatures.ToString());
                if (poolName != null) values.Add("poolName", poolName);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/account/updatesubaccountsettings", values);
            }

        }
        #endregion


        #region Campaign functions
        /// <summary>
        /// Sending and monitoring progress of your Campaigns
        /// </summary>
        public static class Campaign
        {
            /// <summary>
            /// Adds a campaign to the queue for processing based on the configuration
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="campaign">Json representation of a campaign</param>
            /// <returns>int</returns>
            public static async Task<int> AddAsync(ApiTypes.Campaign campaign)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("campaign", Newtonsoft.Json.JsonConvert.SerializeObject(campaign));
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/campaign/add", null, values);
                ApiResponse<int> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<int>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

            /// <summary>
            /// Copy selected campaign
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelID">ID number of selected Channel.</param>
            /// <returns>int</returns>
            public static async Task<int> CopyAsync(int channelID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("channelID", channelID.ToString());
                ApiResponse<int> apiResponse = await ApiUtilities.PostAsync<int>("/campaign/copy", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Delete selected campaign
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelID">ID number of selected Channel.</param>
            public static async Task DeleteAsync(int channelID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("channelID", channelID.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/campaign/delete", values);
            }

            /// <summary>
            /// Export selected campaigns to chosen file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelIDs">List of campaign IDs used for processing</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(IEnumerable<int> channelIDs = null, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (channelIDs != null) values.Add("channelIDs", string.Join(",", channelIDs));
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/campaign/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// List all of your campaigns
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="search">Text fragment used for searching.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <returns>List(ApiTypes.CampaignChannel)</returns>
            public static async Task<List<ApiTypes.CampaignChannel>> ListAsync(string search = null, int offset = 0, int limit = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (search != null) values.Add("search", search);
                if (offset != 0) values.Add("offset", offset.ToString());
                if (limit != 0) values.Add("limit", limit.ToString());
                ApiResponse<List<ApiTypes.CampaignChannel>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.CampaignChannel>>("/campaign/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Updates a previously added campaign.  Only Active and Paused campaigns can be updated.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="campaign">Json representation of a campaign</param>
            /// <returns>int</returns>
            public static async Task<int> UpdateAsync(ApiTypes.Campaign campaign)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("campaign", Newtonsoft.Json.JsonConvert.SerializeObject(campaign));
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/campaign/update", null, values);
                ApiResponse<int> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<int>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

        }
        #endregion


        #region Channel functions
        /// <summary>
        /// SMTP and HTTP API channels for grouping email delivery.
        /// </summary>
        public static class Channel
        {
            /// <summary>
            /// Manually add a channel to your account to group email
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="name">Descriptive name of the channel</param>
            /// <returns>string</returns>
            public static async Task<string> AddAsync(string name)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("name", name);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/channel/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Delete the channel.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="name">The name of the channel to delete.</param>
            public static async Task DeleteAsync(string name)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("name", name);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/channel/delete", values);
            }

            /// <summary>
            /// Export channels in CSV file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelNames">List of channel names used for processing</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.FileData</returns>
            public static async Task<ApiTypes.FileData> ExportCsvAsync(IEnumerable<string> channelNames, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("channelNames", string.Join(",", channelNames));
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                return await ApiUtilities.HttpGetFileAsync("/channel/exportcsv", values);
            }

            /// <summary>
            /// Export channels in JSON file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelNames">List of channel names used for processing</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.FileData</returns>
            public static async Task<ApiTypes.FileData> ExportJsonAsync(IEnumerable<string> channelNames, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("channelNames", string.Join(",", channelNames));
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                return await ApiUtilities.HttpGetFileAsync("/channel/exportjson", values);
            }

            /// <summary>
            /// Export channels in XML file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelNames">List of channel names used for processing</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.FileData</returns>
            public static async Task<ApiTypes.FileData> ExportXmlAsync(IEnumerable<string> channelNames, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("channelNames", string.Join(",", channelNames));
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                return await ApiUtilities.HttpGetFileAsync("/channel/exportxml", values);
            }

            /// <summary>
            /// List all of your channels
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.Channel)</returns>
            public static async Task<List<ApiTypes.Channel>> ListAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.Channel>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Channel>>("/channel/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Rename an existing channel.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="name">The name of the channel to update.</param>
            /// <param name="newName">The new name for the channel.</param>
            /// <returns>string</returns>
            public static async Task<string> UpdateAsync(string name, string newName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("name", name);
                values.Add("newName", newName);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/channel/update", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Contact functions
        /// <summary>
        /// Methods used to manage your Contacts.
        /// </summary>
        public static class Contact
        {
            /// <summary>
            /// Add a new contact and optionally to one of your lists.  Note that your API KEY is not required for this call.
            /// </summary>
            /// <param name="publicAccountID">Public key for limited access to your account such as contact/add so you can use it safely on public websites.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="publicListID">ID code of list</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="firstName">First name.</param>
            /// <param name="lastName">Last name.</param>
            /// <param name="source">Specifies the way of uploading the contact</param>
            /// <param name="returnUrl">URL to navigate to after account creation</param>
            /// <param name="sourceUrl">URL from which request was sent.</param>
            /// <param name="activationReturnUrl">The url to return the contact to after activation.</param>
            /// <param name="activationTemplate"></param>
            /// <param name="sendActivation">True, if you want to send activation email to this account. Otherwise, false</param>
            /// <param name="consentDate">Date of consent to send this contact(s) your email. If not provided current date is used for consent.</param>
            /// <param name="consentIP">IP address of consent to send this contact(s) your email. If not provided your current public IP address is used for consent.</param>
            /// <param name="field">Custom contact field like firstname, lastname, city etc. Request parameters prefixed by field_ like field_firstname, field_lastname </param>
            /// <param name="notifyEmail">Emails, separated by semicolon, to which the notification about contact subscribing should be sent to</param>
            /// <param name="alreadyActiveUrl"></param>
            /// <param name="consentTracking"></param>
            /// <returns>string</returns>
            public static async Task<string> AddAsync(string publicAccountID, string email, IEnumerable<string> publicListID = null, string[] listName = null, string firstName = null, string lastName = null, ApiTypes.ContactSource source = ApiTypes.ContactSource.ContactApi, string returnUrl = null, string sourceUrl = null, string activationReturnUrl = null, string activationTemplate = null, bool sendActivation = true, DateTime? consentDate = null, string consentIP = null, Dictionary<string, string> field = null, string notifyEmail = null, string alreadyActiveUrl = null, ApiTypes.ConsentTracking consentTracking = ApiTypes.ConsentTracking.Unknown)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicAccountID", publicAccountID);
                values.Add("email", email);
                if (publicListID != null) values.Add("publicListID", string.Join(",", publicListID));
                if (listName != null)
                {
                    foreach (string _item in listName)
                    {
                        values.Add("listName", _item);
                    }
                }
                if (firstName != null) values.Add("firstName", firstName);
                if (lastName != null) values.Add("lastName", lastName);
                if (source != ApiTypes.ContactSource.ContactApi) values.Add("source", source.ToString());
                if (returnUrl != null) values.Add("returnUrl", returnUrl);
                if (sourceUrl != null) values.Add("sourceUrl", sourceUrl);
                if (activationReturnUrl != null) values.Add("activationReturnUrl", activationReturnUrl);
                if (activationTemplate != null) values.Add("activationTemplate", activationTemplate);
                if (sendActivation != true) values.Add("sendActivation", sendActivation.ToString());
                if (consentDate != null) values.Add("consentDate", consentDate.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (consentIP != null) values.Add("consentIP", consentIP);
                if (field != null)
                {
                    foreach (KeyValuePair<string, string> _item in field)
                    {
                        values.Add("field_" + _item.Key, _item.Value);
                    }
                }
                if (notifyEmail != null) values.Add("notifyEmail", notifyEmail);
                if (alreadyActiveUrl != null) values.Add("alreadyActiveUrl", alreadyActiveUrl);
                if (consentTracking != ApiTypes.ConsentTracking.Unknown) values.Add("consentTracking", consentTracking.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/contact/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Manually add or update a contacts status to Abuse or Unsubscribed status (blocked).
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="status">Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.</param>
            public static async Task AddBlockedAsync(string email, ApiTypes.ContactStatus status)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                values.Add("status", status.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/contact/addblocked", values);
            }

            /// <summary>
            /// Change any property on the contact record.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="name">Name of the contact property you want to change.</param>
            /// <param name="value">Value you would like to change the contact property to.</param>
            public static async Task ChangePropertyAsync(string email, string name, string value)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                values.Add("name", name);
                values.Add("value", value);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/contact/changeproperty", values);
            }

            /// <summary>
            /// Changes status of selected Contacts. You may provide RULE for selection or specify list of Contact IDs.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="status">Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            public static async Task ChangeStatusAsync(ApiTypes.ContactStatus status, string rule = null, IEnumerable<string> emails = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("status", status.ToString());
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/contact/changestatus", values);
            }

            /// <summary>
            /// Returns number of Contacts, RULE specifies contact Status.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="allContacts">True: Include every Contact in your Account. Otherwise, false</param>
            /// <returns>ApiTypes.ContactStatusCounts</returns>
            public static async Task<ApiTypes.ContactStatusCounts> CountByStatusAsync(string rule = null, bool allContacts = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (rule != null) values.Add("rule", rule);
                if (allContacts != false) values.Add("allContacts", allContacts.ToString());
                ApiResponse<ApiTypes.ContactStatusCounts> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ContactStatusCounts>("/contact/countbystatus", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns count of unsubscribe reasons for unsubscribed and complaint contacts.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>ApiTypes.ContactUnsubscribeReasonCounts</returns>
            public static async Task<ApiTypes.ContactUnsubscribeReasonCounts> CountByUnsubscribeReasonAsync(DateTime? from = null, DateTime? to = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<ApiTypes.ContactUnsubscribeReasonCounts> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ContactUnsubscribeReasonCounts>("/contact/countbyunsubscribereason", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Permanantly deletes the contacts provided.  You can provide either a qualified rule or a list of emails (comma separated string).
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            public static async Task DeleteAsync(string rule = null, IEnumerable<string> emails = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/contact/delete", values);
            }

            /// <summary>
            /// Export selected Contacts to file.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, string rule = null, IEnumerable<string> emails = null, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/contact/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Export contacts' unsubscribe reasons count to file.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportUnsubscribeReasonCountAsync(DateTime? from = null, DateTime? to = null, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/contact/exportunsubscribereasoncount", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Finds all Lists and Segments this email belongs to.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <returns>ApiTypes.ContactCollection</returns>
            public static async Task<ApiTypes.ContactCollection> FindContactAsync(string email)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                ApiResponse<ApiTypes.ContactCollection> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ContactCollection>("/contact/findcontact", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// List of Contacts for provided List
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.Contact)</returns>
            public static async Task<List<ApiTypes.Contact>> GetContactsByListAsync(string listName, int limit = 20, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (limit != 20) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.Contact>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Contact>>("/contact/getcontactsbylist", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// List of Contacts for provided Segment
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentName">Name of your segment.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.Contact)</returns>
            public static async Task<List<ApiTypes.Contact>> GetContactsBySegmentAsync(string segmentName, int limit = 20, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentName", segmentName);
                if (limit != 20) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.Contact>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Contact>>("/contact/getcontactsbysegment", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// List of all contacts. If you have not specified RULE, all Contacts will be listed.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.Contact)</returns>
            public static async Task<List<ApiTypes.Contact>> ListAsync(string rule = null, int limit = 20, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (rule != null) values.Add("rule", rule);
                if (limit != 20) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.Contact>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Contact>>("/contact/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Load blocked contacts
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="statuses">List of blocked statuses: Abuse, Bounced or Unsubscribed</param>
            /// <param name="search">Text fragment used for searching.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.BlockedContact)</returns>
            public static async Task<List<ApiTypes.BlockedContact>> LoadBlockedAsync(IEnumerable<ApiTypes.ContactStatus> statuses, string search = null, int limit = 0, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("statuses", string.Join(",", statuses));
                if (search != null) values.Add("search", search);
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.BlockedContact>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.BlockedContact>>("/contact/loadblocked", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Load detailed contact information
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <returns>ApiTypes.Contact</returns>
            public static async Task<ApiTypes.Contact> LoadContactAsync(string email)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                ApiResponse<ApiTypes.Contact> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Contact>("/contact/loadcontact", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows detailed history of chosen Contact.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.ContactHistory)</returns>
            public static async Task<List<ApiTypes.ContactHistory>> LoadHistoryAsync(string email, int limit = 0, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.ContactHistory>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.ContactHistory>>("/contact/loadhistory", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Add new Contact to one of your Lists.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            /// <param name="firstName">First name.</param>
            /// <param name="lastName">Last name.</param>
            /// <param name="publicListID">ID code of list</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="status">Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.</param>
            /// <param name="notes">Free form field of notes</param>
            /// <param name="consentDate">Date of consent to send this contact(s) your email. If not provided current date is used for consent.</param>
            /// <param name="consentIP">IP address of consent to send this contact(s) your email. If not provided your current public IP address is used for consent.</param>
            /// <param name="field">Custom contact field like firstname, lastname, city etc. Request parameters prefixed by field_ like field_firstname, field_lastname </param>
            /// <param name="notifyEmail">Emails, separated by semicolon, to which the notification about contact subscribing should be sent to</param>
            /// <param name="consentTracking"></param>
            public static async Task QuickAddAsync(IEnumerable<string> emails, string firstName = null, string lastName = null, string publicListID = null, string listName = null, ApiTypes.ContactStatus status = ApiTypes.ContactStatus.Active, string notes = null, DateTime? consentDate = null, string consentIP = null, Dictionary<string, string> field = null, string notifyEmail = null, ApiTypes.ConsentTracking consentTracking = ApiTypes.ConsentTracking.Unknown)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("emails", string.Join(",", emails));
                if (firstName != null) values.Add("firstName", firstName);
                if (lastName != null) values.Add("lastName", lastName);
                if (publicListID != null) values.Add("publicListID", publicListID);
                if (listName != null) values.Add("listName", listName);
                if (status != ApiTypes.ContactStatus.Active) values.Add("status", status.ToString());
                if (notes != null) values.Add("notes", notes);
                if (consentDate != null) values.Add("consentDate", consentDate.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (consentIP != null) values.Add("consentIP", consentIP);
                if (field != null)
                {
                    foreach (KeyValuePair<string, string> _item in field)
                    {
                        values.Add("field_" + _item.Key, _item.Value);
                    }
                }
                if (notifyEmail != null) values.Add("notifyEmail", notifyEmail);
                if (consentTracking != ApiTypes.ConsentTracking.Unknown) values.Add("consentTracking", consentTracking.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/contact/quickadd", values);
            }

            /// <summary>
            /// Basic double opt-in email subscribe form for your account.  This can be used for contacts that need to re-subscribe as well.
            /// </summary>
            /// <param name="publicAccountID">Public key for limited access to your account such as contact/add so you can use it safely on public websites.</param>
            /// <returns>string</returns>
            public static async Task<string> SubscribeAsync(string publicAccountID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicAccountID", publicAccountID);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/contact/subscribe", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Update selected contact. Omitted contact's fields will be reset by default (see the clearRestOfFields parameter)
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="firstName">First name.</param>
            /// <param name="lastName">Last name.</param>
            /// <param name="clearRestOfFields">States if the fields that were omitted in this request are to be reset or should they be left with their current value</param>
            /// <param name="field">Custom contact field like firstname, lastname, city etc. Request parameters prefixed by field_ like field_firstname, field_lastname </param>
            /// <param name="customFields">Custom contact field like firstname, lastname, city etc. JSON serialized text like { "city":"london" } </param>
            /// <returns>ApiTypes.Contact</returns>
            public static async Task<ApiTypes.Contact> UpdateAsync(string email, string firstName = null, string lastName = null, bool clearRestOfFields = true, Dictionary<string, string> field = null, string customFields = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("email", email);
                if (firstName != null) values.Add("firstName", firstName);
                if (lastName != null) values.Add("lastName", lastName);
                if (clearRestOfFields != true) values.Add("clearRestOfFields", clearRestOfFields.ToString());
                if (field != null)
                {
                    foreach (KeyValuePair<string, string> _item in field)
                    {
                        values.Add("field_" + _item.Key, _item.Value);
                    }
                }
                if (customFields != null) values.Add("customFields", customFields);
                ApiResponse<ApiTypes.Contact> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Contact>("/contact/update", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Upload contacts in CSV file.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="contactFile">Name of CSV file with Contacts.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this (optional) newly created list. Otherwise, false</param>
            /// <param name="listID">ID number of selected list.</param>
            /// <param name="listName">Name of your list to upload contacts to, or how the new, automatically created list should be named</param>
            /// <param name="status">Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.</param>
            /// <param name="consentDate">Date of consent to send this contact(s) your email. If not provided current date is used for consent.</param>
            /// <param name="consentIP">IP address of consent to send this contact(s) your email. If not provided your current public IP address is used for consent.</param>
            /// <param name="consentTracking"></param>
            /// <returns>int</returns>
            public static async Task<int> UploadAsync(ApiTypes.FileData contactFile, bool allowUnsubscribe = false, int? listID = null, string listName = null, ApiTypes.ContactStatus status = ApiTypes.ContactStatus.Active, DateTime? consentDate = null, string consentIP = null, ApiTypes.ConsentTracking consentTracking = ApiTypes.ConsentTracking.Unknown)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (allowUnsubscribe != false) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                if (listID != null) values.Add("listID", listID.ToString());
                if (listName != null) values.Add("listName", listName);
                if (status != ApiTypes.ContactStatus.Active) values.Add("status", status.ToString());
                if (consentDate != null) values.Add("consentDate", consentDate.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (consentIP != null) values.Add("consentIP", consentIP);
                if (consentTracking != ApiTypes.ConsentTracking.Unknown) values.Add("consentTracking", consentTracking.ToString());
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/contact/upload", new List<ApiTypes.FileData>() { contactFile }, values);
                ApiResponse<int> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<int>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

        }
        #endregion


        #region Domain functions
        /// <summary>
        /// Managing sender domains. Creating new entries and validating domain records.
        /// </summary>
        public static class Domain
        {
            /// <summary>
            /// Add new domain to account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            /// <param name="trackingType"></param>
            public static async Task AddAsync(string domain, ApiTypes.TrackingType trackingType = ApiTypes.TrackingType.Http)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                if (trackingType != ApiTypes.TrackingType.Http) values.Add("trackingType", trackingType.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/domain/add", values);
            }

            /// <summary>
            /// Deletes configured domain from account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            public static async Task DeleteAsync(string domain)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/domain/delete", values);
            }

            /// <summary>
            /// Lists all domains configured for this account.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.DomainDetail)</returns>
            public static async Task<List<ApiTypes.DomainDetail>> ListAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.DomainDetail>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.DomainDetail>>("/domain/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Verification of email addres set for domain.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Default email sender, example: mail@yourdomain.com</param>
            public static async Task SetDefaultAsync(string domain)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/domain/setdefault", values);
            }

            /// <summary>
            /// Verification of DKIM record for domain
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            /// <returns>string</returns>
            public static async Task<string> VerifyDkimAsync(string domain)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/domain/verifydkim", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Verification of MX record for domain
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            /// <returns>string</returns>
            public static async Task<string> VerifyMXAsync(string domain)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/domain/verifymx", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Verification of SPF record for domain
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            /// <returns>string</returns>
            public static async Task<string> VerifySpfAsync(string domain)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/domain/verifyspf", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Verification of tracking CNAME record for domain
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="domain">Name of selected domain.</param>
            /// <param name="trackingType"></param>
            /// <returns>string</returns>
            public static async Task<string> VerifyTrackingAsync(string domain, ApiTypes.TrackingType trackingType = ApiTypes.TrackingType.Http)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("domain", domain);
                if (trackingType != ApiTypes.TrackingType.Http) values.Add("trackingType", trackingType.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/domain/verifytracking", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Email functions
        /// <summary>
        /// 
        /// </summary>
        public static class Email
        {
            /// <summary>
            /// Get email batch status
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="transactionID">Transaction identifier</param>
            /// <param name="showFailed">Include Bounced email addresses.</param>
            /// <param name="showSent">Include Sent email addresses.</param>
            /// <param name="showDelivered">Include all delivered email addresses.</param>
            /// <param name="showPending">Include Ready to send email addresses.</param>
            /// <param name="showOpened">Include Opened email addresses.</param>
            /// <param name="showClicked">Include Clicked email addresses.</param>
            /// <param name="showAbuse">Include Reported as abuse email addresses.</param>
            /// <param name="showUnsubscribed">Include Unsubscribed email addresses.</param>
            /// <param name="showErrors">Include error messages for bounced emails.</param>
            /// <param name="showMessageIDs">Include all MessageIDs for this transaction</param>
            /// <returns>ApiTypes.EmailJobStatus</returns>
            public static async Task<ApiTypes.EmailJobStatus> GetStatusAsync(string transactionID, bool showFailed = false, bool showSent = false, bool showDelivered = false, bool showPending = false, bool showOpened = false, bool showClicked = false, bool showAbuse = false, bool showUnsubscribed = false, bool showErrors = false, bool showMessageIDs = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("transactionID", transactionID);
                if (showFailed != false) values.Add("showFailed", showFailed.ToString());
                if (showSent != false) values.Add("showSent", showSent.ToString());
                if (showDelivered != false) values.Add("showDelivered", showDelivered.ToString());
                if (showPending != false) values.Add("showPending", showPending.ToString());
                if (showOpened != false) values.Add("showOpened", showOpened.ToString());
                if (showClicked != false) values.Add("showClicked", showClicked.ToString());
                if (showAbuse != false) values.Add("showAbuse", showAbuse.ToString());
                if (showUnsubscribed != false) values.Add("showUnsubscribed", showUnsubscribed.ToString());
                if (showErrors != false) values.Add("showErrors", showErrors.ToString());
                if (showMessageIDs != false) values.Add("showMessageIDs", showMessageIDs.ToString());
                ApiResponse<ApiTypes.EmailJobStatus> apiResponse = await ApiUtilities.PostAsync<ApiTypes.EmailJobStatus>("/email/getstatus", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Submit emails. The HTTP POST request is suggested. The default, maximum (accepted by us) size of an email is 10 MB in total, with or without attachments included. For suggested implementations please refer to https://elasticemail.com/support/http-api/
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="subject">Email subject</param>
            /// <param name="from">From email address</param>
            /// <param name="fromName">Display name for from email address</param>
            /// <param name="sender">Email address of the sender</param>
            /// <param name="senderName">Display name sender</param>
            /// <param name="msgFrom">Optional parameter. Sets FROM MIME header.</param>
            /// <param name="msgFromName">Optional parameter. Sets FROM name of MIME header.</param>
            /// <param name="replyTo">Email address to reply to</param>
            /// <param name="replyToName">Display name of the reply to address</param>
            /// <param name="to">List of email recipients (each email is treated separately, like a BCC). Separated by comma or semicolon. We suggest using the "msgTo" parameter if backward compatibility with API version 1 is not a must.</param>
            /// <param name="msgTo">Optional parameter. Will be ignored if the 'to' parameter is also provided. List of email recipients (visible to all other recipients of the message as TO MIME header). Separated by comma or semicolon.</param>
            /// <param name="msgCC">Optional parameter. Will be ignored if the 'to' parameter is also provided. List of email recipients (visible to all other recipients of the message as CC MIME header). Separated by comma or semicolon.</param>
            /// <param name="msgBcc">Optional parameter. Will be ignored if the 'to' parameter is also provided. List of email recipients (each email is treated seperately). Separated by comma or semicolon.</param>
            /// <param name="lists">The name of a contact list you would like to send to. Separate multiple contact lists by commas or semicolons.</param>
            /// <param name="segments">The name of a segment you would like to send to. Separate multiple segments by comma or semicolon. Insert "0" for all Active contacts.</param>
            /// <param name="mergeSourceFilename">File name one of attachments which is a CSV list of Recipients.</param>
            /// <param name="dataSource">Name or ID of the previously uploaded file which should be a CSV list of Recipients.</param>
            /// <param name="channel">An ID field (max 191 chars) that can be used for reporting [will default to HTTP API or SMTP API]</param>
            /// <param name="bodyHtml">Html email body</param>
            /// <param name="bodyText">Text email body</param>
            /// <param name="charset">Text value of charset encoding for example: iso-8859-1, windows-1251, utf-8, us-ascii, windows-1250 and more…</param>
            /// <param name="charsetBodyHtml">Sets charset for body html MIME part (overrides default value from charset parameter)</param>
            /// <param name="charsetBodyText">Sets charset for body text MIME part (overrides default value from charset parameter)</param>
            /// <param name="encodingType">0 for None, 1 for Raw7Bit, 2 for Raw8Bit, 3 for QuotedPrintable, 4 for Base64 (Default), 5 for Uue  note that you can also provide the text version such as "Raw7Bit" for value 1.  NOTE: Base64 or QuotedPrintable is recommended if you are validating your domain(s) with DKIM.</param>
            /// <param name="template">The ID of an email template you have created in your account.</param>
            /// <param name="attachmentFiles">Attachment files. These files should be provided with the POST multipart file upload, not directly in the request's URL. Can also include merge CSV file</param>
            /// <param name="headers">Optional Custom Headers. Request parameters prefixed by headers_ like headers_customheader1, headers_customheader2. Note: a space is required after the colon before the custom header value. headers_xmailer=xmailer: header-value1</param>
            /// <param name="postBack">Optional header returned in notifications.</param>
            /// <param name="merge">Request parameters prefixed by merge_ like merge_firstname, merge_lastname. If sending to a template you can send merge_ fields to merge data with the template. Template fields are entered with {firstname}, {lastname} etc.</param>
            /// <param name="timeOffSetMinutes">Number of minutes in the future this email should be sent up to a maximum of 1 year (524160 minutes)</param>
            /// <param name="poolName">Name of your custom IP Pool to be used in the sending process</param>
            /// <param name="isTransactional">True, if email is transactional (non-bulk, non-marketing, non-commercial). Otherwise, false</param>
            /// <param name="attachments">Names or IDs of attachments previously uploaded to your account that should be sent with this e-mail.</param>
            /// <param name="trackOpens">Should the opens be tracked? If no value has been provided, account's default setting will be used.</param>
            /// <param name="trackClicks">Should the clicks be tracked? If no value has been provided, account's default setting will be used.</param>
            /// <returns>ApiTypes.EmailSend</returns>
            public static async Task<ApiTypes.EmailSend> SendAsync(string subject = null, string from = null, string fromName = null, string sender = null, string senderName = null, string msgFrom = null, string msgFromName = null, string replyTo = null, string replyToName = null, IEnumerable<string> to = null, IEnumerable<string> msgTo = null, IEnumerable<string> msgCC = null, IEnumerable<string> msgBcc = null, IEnumerable<string> lists = null, IEnumerable<string> segments = null, string mergeSourceFilename = null, string dataSource = null, string channel = null, string bodyHtml = null, string bodyText = null, string charset = null, string charsetBodyHtml = null, string charsetBodyText = null, ApiTypes.EncodingType encodingType = ApiTypes.EncodingType.None, string template = null, IEnumerable<ApiTypes.FileData> attachmentFiles = null, Dictionary<string, string> headers = null, string postBack = null, Dictionary<string, string> merge = null, string timeOffSetMinutes = null, string poolName = null, bool isTransactional = false, IEnumerable<string> attachments = null, bool? trackOpens = null, bool? trackClicks = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (subject != null) values.Add("subject", subject);
                if (from != null) values.Add("from", from);
                if (fromName != null) values.Add("fromName", fromName);
                if (sender != null) values.Add("sender", sender);
                if (senderName != null) values.Add("senderName", senderName);
                if (msgFrom != null) values.Add("msgFrom", msgFrom);
                if (msgFromName != null) values.Add("msgFromName", msgFromName);
                if (replyTo != null) values.Add("replyTo", replyTo);
                if (replyToName != null) values.Add("replyToName", replyToName);
                if (to != null) values.Add("to", string.Join(",", to));
                if (msgTo != null) values.Add("msgTo", string.Join(",", msgTo));
                if (msgCC != null) values.Add("msgCC", string.Join(",", msgCC));
                if (msgBcc != null) values.Add("msgBcc", string.Join(",", msgBcc));
                if (lists != null) values.Add("lists", string.Join(",", lists));
                if (segments != null) values.Add("segments", string.Join(",", segments));
                if (mergeSourceFilename != null) values.Add("mergeSourceFilename", mergeSourceFilename);
                if (dataSource != null) values.Add("dataSource", dataSource);
                if (channel != null) values.Add("channel", channel);
                if (bodyHtml != null) values.Add("bodyHtml", bodyHtml);
                if (bodyText != null) values.Add("bodyText", bodyText);
                if (charset != null) values.Add("charset", charset);
                if (charsetBodyHtml != null) values.Add("charsetBodyHtml", charsetBodyHtml);
                if (charsetBodyText != null) values.Add("charsetBodyText", charsetBodyText);
                if (encodingType != ApiTypes.EncodingType.None) values.Add("encodingType", encodingType.ToString());
                if (template != null) values.Add("template", template);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> _item in headers)
                    {
                        values.Add("headers_" + _item.Key, _item.Value);
                    }
                }
                if (postBack != null) values.Add("postBack", postBack);
                if (merge != null)
                {
                    foreach (KeyValuePair<string, string> _item in merge)
                    {
                        values.Add("merge_" + _item.Key, _item.Value);
                    }
                }
                if (timeOffSetMinutes != null) values.Add("timeOffSetMinutes", timeOffSetMinutes);
                if (poolName != null) values.Add("poolName", poolName);
                if (isTransactional != false) values.Add("isTransactional", isTransactional.ToString());
                if (attachments != null) values.Add("attachments", string.Join(",", attachments));
                if (trackOpens != null) values.Add("trackOpens", trackOpens.ToString());
                if (trackClicks != null) values.Add("trackClicks", trackClicks.ToString());
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/email/send", attachmentFiles == null ? null : attachmentFiles.ToList(), values);
                ApiResponse<ApiTypes.EmailSend> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<ApiTypes.EmailSend>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

            /// <summary>
            /// Detailed status of a unique email sent through your account. Returns a 'Email has expired and the status is unknown.' error, if the email has not been fully processed yet.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="messageID">Unique identifier for this email.</param>
            /// <returns>ApiTypes.EmailStatus</returns>
            public static async Task<ApiTypes.EmailStatus> StatusAsync(string messageID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("messageID", messageID);
                ApiResponse<ApiTypes.EmailStatus> apiResponse = await ApiUtilities.PostAsync<ApiTypes.EmailStatus>("/email/status", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// View email
            /// </summary>
            /// <param name="messageID">Message identifier</param>
            /// <returns>ApiTypes.EmailView</returns>
            public static async Task<ApiTypes.EmailView> ViewAsync(string messageID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("messageID", messageID);
                ApiResponse<ApiTypes.EmailView> apiResponse = await ApiUtilities.PostAsync<ApiTypes.EmailView>("/email/view", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Export functions
        /// <summary>
        /// 
        /// </summary>
        public static class Export
        {
            /// <summary>
            /// Check the current status of the export.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicExportID"></param>
            /// <returns>ApiTypes.ExportStatus</returns>
            public static async Task<ApiTypes.ExportStatus> CheckStatusAsync(Guid publicExportID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicExportID", publicExportID.ToString());
                ApiResponse<ApiTypes.ExportStatus> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportStatus>("/export/checkstatus", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Summary of export type counts.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>ApiTypes.ExportTypeCounts</returns>
            public static async Task<ApiTypes.ExportTypeCounts> CountByTypeAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<ApiTypes.ExportTypeCounts> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportTypeCounts>("/export/countbytype", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Delete the specified export.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicExportID"></param>
            public static async Task DeleteAsync(Guid publicExportID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicExportID", publicExportID.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/export/delete", values);
            }

            /// <summary>
            /// Returns a list of all exported data.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>List(ApiTypes.Export)</returns>
            public static async Task<List<ApiTypes.Export>> ListAsync(int limit = 0, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<List<ApiTypes.Export>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Export>>("/export/list", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region File functions
        /// <summary>
        /// Manage the files on your account
        /// </summary>
        public static class File
        {
            /// <summary>
            /// Permanently deletes the file from your account
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="fileID"></param>
            /// <param name="filename">Name of your file.</param>
            public static async Task DeleteAsync(int? fileID = null, string filename = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (fileID != null) values.Add("fileID", fileID.ToString());
                if (filename != null) values.Add("filename", filename);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/file/delete", values);
            }

            /// <summary>
            /// Gets content of the chosen File
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="filename">Name of your file.</param>
            /// <param name="fileID"></param>
            /// <returns>ApiTypes.FileData</returns>
            public static async Task<ApiTypes.FileData> DownloadAsync(string filename = null, int? fileID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (filename != null) values.Add("filename", filename);
                if (fileID != null) values.Add("fileID", fileID.ToString());
                return await ApiUtilities.HttpGetFileAsync("/file/download", values);
            }

            /// <summary>
            /// Lists your available Attachments in the given email
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="msgID">ID number of selected message.</param>
            /// <returns>List(ApiTypes.File)</returns>
            public static async Task<List<ApiTypes.File>> ListAsync(string msgID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("msgID", msgID);
                ApiResponse<List<ApiTypes.File>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.File>>("/file/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists all your available files
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.File)</returns>
            public static async Task<List<ApiTypes.File>> ListAllAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.File>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.File>>("/file/listall", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Gets chosen File
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="filename">Name of your file.</param>
            /// <returns>ApiTypes.File</returns>
            public static async Task<ApiTypes.File> LoadAsync(string filename)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("filename", filename);
                ApiResponse<ApiTypes.File> apiResponse = await ApiUtilities.PostAsync<ApiTypes.File>("/file/load", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Uploads selected file to the server using http form upload format (MIME multipart/form-data) or PUT method.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="file"></param>
            /// <param name="name">Filename</param>
            /// <param name="expiresAfterDays">After how many days should the file be deleted.</param>
            /// <returns>ApiTypes.File</returns>
            public static async Task<ApiTypes.File> UploadAsync(ApiTypes.FileData file, string name = null, int? expiresAfterDays = 35)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (name != null) values.Add("name", name);
                if (expiresAfterDays != 35) values.Add("expiresAfterDays", expiresAfterDays.ToString());
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/file/upload", new List<ApiTypes.FileData>() { file }, values);
                ApiResponse<ApiTypes.File> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<ApiTypes.File>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

        }
        #endregion


        #region List functions
        /// <summary>
        /// API methods for managing your Lists
        /// </summary>
        public static class List
        {
            /// <summary>
            /// Create new list, based on filtering rule or list of IDs
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="createEmptyList">True to create an empty list, otherwise false. Ignores rule and emails parameters if provided.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this list. Otherwise, false</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            /// <param name="allContacts">True: Include every Contact in your Account. Otherwise, false</param>
            /// <returns>int</returns>
            public static async Task<int> AddAsync(string listName, bool createEmptyList = false, bool allowUnsubscribe = false, string rule = null, IEnumerable<string> emails = null, bool allContacts = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (createEmptyList != false) values.Add("createEmptyList", createEmptyList.ToString());
                if (allowUnsubscribe != false) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                if (allContacts != false) values.Add("allContacts", allContacts.ToString());
                ApiResponse<int> apiResponse = await ApiUtilities.PostAsync<int>("/list/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Add existing Contacts to chosen list
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            /// <param name="allContacts">True: Include every Contact in your Account. Otherwise, false</param>
            public static async Task AddContactsAsync(string listName, string rule = null, IEnumerable<string> emails = null, bool allContacts = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                if (allContacts != false) values.Add("allContacts", allContacts.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/addcontacts", values);
            }

            /// <summary>
            /// Copy your existing List with the option to provide new settings to it. Some fields, when left empty, default to the source list's settings
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="sourceListName">The name of the list you want to copy</param>
            /// <param name="newlistName">Name of your list if you want to change it.</param>
            /// <param name="createEmptyList">True to create an empty list, otherwise false. Ignores rule and emails parameters if provided.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this list. Otherwise, false</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <returns>int</returns>
            public static async Task<int> CopyAsync(string sourceListName, string newlistName = null, bool? createEmptyList = null, bool? allowUnsubscribe = null, string rule = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("sourceListName", sourceListName);
                if (newlistName != null) values.Add("newlistName", newlistName);
                if (createEmptyList != null) values.Add("createEmptyList", createEmptyList.ToString());
                if (allowUnsubscribe != null) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                if (rule != null) values.Add("rule", rule);
                ApiResponse<int> apiResponse = await ApiUtilities.PostAsync<int>("/list/copy", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Create a new list from the recipients of the given campaign, using the given statuses of Messages
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="campaignID">ID of the campaign which recipients you want to copy</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="statuses">Statuses of a campaign's emails you want to include in the new list (but NOT the contacts' statuses)</param>
            /// <returns>int</returns>
            public static async Task<int> CreateFromCampaignAsync(int campaignID, string listName, IEnumerable<ApiTypes.LogJobStatus> statuses = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("campaignID", campaignID.ToString());
                values.Add("listName", listName);
                if (statuses != null) values.Add("statuses", string.Join(",", statuses));
                ApiResponse<int> apiResponse = await ApiUtilities.PostAsync<int>("/list/createfromcampaign", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Create a series of nth selection lists from an existing list or segment
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="numberOfLists">The number of evenly distributed lists to create.</param>
            /// <param name="excludeBlocked">True if you want to exclude contacts that are currently in a blocked status of either unsubscribe, complaint or bounce. Otherwise, false.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this list. Otherwise, false</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="allContacts">True: Include every Contact in your Account. Otherwise, false</param>
            public static async Task CreateNthSelectionListsAsync(string listName, int numberOfLists, bool excludeBlocked = true, bool allowUnsubscribe = false, string rule = null, bool allContacts = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                values.Add("numberOfLists", numberOfLists.ToString());
                if (excludeBlocked != true) values.Add("excludeBlocked", excludeBlocked.ToString());
                if (allowUnsubscribe != false) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                if (rule != null) values.Add("rule", rule);
                if (allContacts != false) values.Add("allContacts", allContacts.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/createnthselectionlists", values);
            }

            /// <summary>
            /// Create a new list with randomized contacts from an existing list or segment
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="count">Number of items.</param>
            /// <param name="excludeBlocked">True if you want to exclude contacts that are currently in a blocked status of either unsubscribe, complaint or bounce. Otherwise, false.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this list. Otherwise, false</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="allContacts">True: Include every Contact in your Account. Otherwise, false</param>
            /// <returns>int</returns>
            public static async Task<int> CreateRandomListAsync(string listName, int count, bool excludeBlocked = true, bool allowUnsubscribe = false, string rule = null, bool allContacts = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                values.Add("count", count.ToString());
                if (excludeBlocked != true) values.Add("excludeBlocked", excludeBlocked.ToString());
                if (allowUnsubscribe != false) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                if (rule != null) values.Add("rule", rule);
                if (allContacts != false) values.Add("allContacts", allContacts.ToString());
                ApiResponse<int> apiResponse = await ApiUtilities.PostAsync<int>("/list/createrandomlist", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Deletes List and removes all the Contacts from it (does not delete Contacts).
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            public static async Task DeleteAsync(string listName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/delete", values);
            }

            /// <summary>
            /// Exports all the contacts from the provided list
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(string listName, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/list/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows all your existing lists
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>List(ApiTypes.List)</returns>
            public static async Task<List<ApiTypes.List>> listAsync(DateTime? from = null, DateTime? to = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<List<ApiTypes.List>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.List>>("/list/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns detailed information about specific list.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <returns>ApiTypes.List</returns>
            public static async Task<ApiTypes.List> LoadAsync(string listName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                ApiResponse<ApiTypes.List> apiResponse = await ApiUtilities.PostAsync<ApiTypes.List>("/list/load", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Move selected contacts from one List to another
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="oldListName">The name of the list from which the contacts will be copied from</param>
            /// <param name="newListName">The name of the list to copy the contacts to</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            /// <param name="moveAll">TRUE - moves all contacts; FALSE - moves contacts provided in the 'emails' parameter. This is ignored if the 'statuses' parameter has been provided</param>
            /// <param name="statuses">List of contact statuses which are eligible to move. This ignores the 'moveAll' parameter</param>
            /// <param name="rule">Query used for filtering.</param>
            public static async Task MoveContactsAsync(string oldListName, string newListName, IEnumerable<string> emails = null, bool? moveAll = null, IEnumerable<ApiTypes.ContactStatus> statuses = null, string rule = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("oldListName", oldListName);
                values.Add("newListName", newListName);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                if (moveAll != null) values.Add("moveAll", moveAll.ToString());
                if (statuses != null) values.Add("statuses", string.Join(",", statuses));
                if (rule != null) values.Add("rule", rule);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/movecontacts", values);
            }

            /// <summary>
            /// Remove selected Contacts from your list
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <param name="emails">Comma delimited list of contact emails</param>
            public static async Task RemoveContactsAsync(string listName, string rule = null, IEnumerable<string> emails = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (rule != null) values.Add("rule", rule);
                if (emails != null) values.Add("emails", string.Join(",", emails));
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/removecontacts", values);
            }

            /// <summary>
            /// Update existing list
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="listName">Name of your list.</param>
            /// <param name="newListName">Name of your list if you want to change it.</param>
            /// <param name="allowUnsubscribe">True: Allow unsubscribing from this list. Otherwise, false</param>
            public static async Task UpdateAsync(string listName, string newListName = null, bool allowUnsubscribe = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("listName", listName);
                if (newListName != null) values.Add("newListName", newListName);
                if (allowUnsubscribe != false) values.Add("allowUnsubscribe", allowUnsubscribe.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/list/update", values);
            }

        }
        #endregion


        #region Log functions
        /// <summary>
        /// Methods to check logs of your campaigns
        /// </summary>
        public static class Log
        {
            /// <summary>
            /// Cancels emails that are waiting to be sent.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <param name="transactionID">ID number of transaction</param>
            public static async Task CancelInProgressAsync(string channelName = null, string transactionID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (channelName != null) values.Add("channelName", channelName);
                if (transactionID != null) values.Add("transactionID", transactionID);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/log/cancelinprogress", values);
            }

            /// <summary>
            /// Export email log information to the specified file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="statuses">List of comma separated message statuses: 0 for all, 1 for ReadyToSend, 2 for InProgress, 4 for Bounced, 5 for Sent, 6 for Opened, 7 for Clicked, 8 for Unsubscribed, 9 for Abuse Report</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="from">Start date.</param>
            /// <param name="to">End date.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <param name="includeEmail">True: Search includes emails. Otherwise, false.</param>
            /// <param name="includeSms">True: Search includes SMS. Otherwise, false.</param>
            /// <param name="messageCategory">ID of message category</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <param name="email">Proper email address.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(IEnumerable<ApiTypes.LogJobStatus> statuses, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, DateTime? from = null, DateTime? to = null, string channelName = null, bool includeEmail = true, bool includeSms = true, IEnumerable<ApiTypes.MessageCategory> messageCategory = null, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null, string email = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("statuses", string.Join(",", statuses));
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (channelName != null) values.Add("channelName", channelName);
                if (includeEmail != true) values.Add("includeEmail", includeEmail.ToString());
                if (includeSms != true) values.Add("includeSms", includeSms.ToString());
                if (messageCategory != null) values.Add("messageCategory", string.Join(",", messageCategory));
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                if (email != null) values.Add("email", email);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/log/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Export detailed link tracking information to the specified file format.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportLinkTrackingAsync(DateTime? from, DateTime? to, string channelName = null, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, int limit = 0, int offset = 0, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (channelName != null) values.Add("channelName", channelName);
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/log/exportlinktracking", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Track link clicks
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <returns>ApiTypes.LinkTrackingDetails</returns>
            public static async Task<ApiTypes.LinkTrackingDetails> LinkTrackingAsync(DateTime? from = null, DateTime? to = null, int limit = 0, int offset = 0, string channelName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                if (channelName != null) values.Add("channelName", channelName);
                ApiResponse<ApiTypes.LinkTrackingDetails> apiResponse = await ApiUtilities.PostAsync<ApiTypes.LinkTrackingDetails>("/log/linktracking", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns logs filtered by specified parameters.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="statuses">List of comma separated message statuses: 0 for all, 1 for ReadyToSend, 2 for InProgress, 4 for Bounced, 5 for Sent, 6 for Opened, 7 for Clicked, 8 for Unsubscribed, 9 for Abuse Report</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="includeEmail">True: Search includes emails. Otherwise, false.</param>
            /// <param name="includeSms">True: Search includes SMS. Otherwise, false.</param>
            /// <param name="messageCategory">ID of message category</param>
            /// <param name="email">Proper email address.</param>
            /// <param name="useStatusChangeDate">True, if 'from' and 'to' parameters should resolve to the Status Change date. To resolve to the creation date - false</param>
            /// <returns>ApiTypes.Log</returns>
            public static async Task<ApiTypes.Log> LoadAsync(IEnumerable<ApiTypes.LogJobStatus> statuses, DateTime? from = null, DateTime? to = null, string channelName = null, int limit = 0, int offset = 0, bool includeEmail = true, bool includeSms = true, IEnumerable<ApiTypes.MessageCategory> messageCategory = null, string email = null, bool useStatusChangeDate = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("statuses", string.Join(",", statuses));
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (channelName != null) values.Add("channelName", channelName);
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                if (includeEmail != true) values.Add("includeEmail", includeEmail.ToString());
                if (includeSms != true) values.Add("includeSms", includeSms.ToString());
                if (messageCategory != null) values.Add("messageCategory", string.Join(",", messageCategory));
                if (email != null) values.Add("email", email);
                if (useStatusChangeDate != false) values.Add("useStatusChangeDate", useStatusChangeDate.ToString());
                ApiResponse<ApiTypes.Log> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Log>("/log/load", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Returns notification logs filtered by specified parameters.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="statuses">List of comma separated message statuses: 0 for all, 1 for ReadyToSend, 2 for InProgress, 4 for Bounced, 5 for Sent, 6 for Opened, 7 for Clicked, 8 for Unsubscribed, 9 for Abuse Report</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <param name="messageCategory">ID of message category</param>
            /// <param name="useStatusChangeDate">True, if 'from' and 'to' parameters should resolve to the Status Change date. To resolve to the creation date - false</param>
            /// <param name="notificationType"></param>
            /// <returns>ApiTypes.Log</returns>
            public static async Task<ApiTypes.Log> LoadNotificationsAsync(IEnumerable<ApiTypes.LogJobStatus> statuses, DateTime? from = null, DateTime? to = null, int limit = 0, int offset = 0, IEnumerable<ApiTypes.MessageCategory> messageCategory = null, bool useStatusChangeDate = false, ApiTypes.NotificationType notificationType = ApiTypes.NotificationType.All)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("statuses", string.Join(",", statuses));
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (limit != 0) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                if (messageCategory != null) values.Add("messageCategory", string.Join(",", messageCategory));
                if (useStatusChangeDate != false) values.Add("useStatusChangeDate", useStatusChangeDate.ToString());
                if (notificationType != ApiTypes.NotificationType.All) values.Add("notificationType", notificationType.ToString());
                ApiResponse<ApiTypes.Log> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Log>("/log/loadnotifications", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Retry sending of temporarily not delivered message.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="msgID">ID number of selected message.</param>
            public static async Task RetryNowAsync(string msgID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("msgID", msgID);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/log/retrynow", values);
            }

            /// <summary>
            /// Loads summary information about activity in chosen date range.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="from">Starting date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">Ending date for search in YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="channelName">Name of selected channel.</param>
            /// <param name="interval">'Hourly' for detailed information, 'summary' for daily overview</param>
            /// <param name="transactionID">ID number of transaction</param>
            /// <returns>ApiTypes.LogSummary</returns>
            public static async Task<ApiTypes.LogSummary> SummaryAsync(DateTime from, DateTime to, string channelName = null, ApiTypes.IntervalType interval = ApiTypes.IntervalType.Summary, string transactionID = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("from", from.ToString("M/d/yyyy h:mm:ss tt"));
                values.Add("to", to.ToString("M/d/yyyy h:mm:ss tt"));
                if (channelName != null) values.Add("channelName", channelName);
                if (interval != ApiTypes.IntervalType.Summary) values.Add("interval", interval.ToString());
                if (transactionID != null) values.Add("transactionID", transactionID);
                ApiResponse<ApiTypes.LogSummary> apiResponse = await ApiUtilities.PostAsync<ApiTypes.LogSummary>("/log/summary", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Segment functions
        /// <summary>
        /// Manages your segments - dynamically created lists of contacts
        /// </summary>
        public static class Segment
        {
            /// <summary>
            /// Create new segment, based on specified RULE.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentName">Name of your segment.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <returns>ApiTypes.Segment</returns>
            public static async Task<ApiTypes.Segment> AddAsync(string segmentName, string rule)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentName", segmentName);
                values.Add("rule", rule);
                ApiResponse<ApiTypes.Segment> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Segment>("/segment/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Copy your existing Segment with the optional new rule and custom name
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="sourceSegmentName">The name of the segment you want to copy</param>
            /// <param name="newSegmentName">New name of your segment if you want to change it.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <returns>ApiTypes.Segment</returns>
            public static async Task<ApiTypes.Segment> CopyAsync(string sourceSegmentName, string newSegmentName = null, string rule = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("sourceSegmentName", sourceSegmentName);
                if (newSegmentName != null) values.Add("newSegmentName", newSegmentName);
                if (rule != null) values.Add("rule", rule);
                ApiResponse<ApiTypes.Segment> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Segment>("/segment/copy", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Delete existing segment.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentName">Name of your segment.</param>
            public static async Task DeleteAsync(string segmentName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentName", segmentName);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/segment/delete", values);
            }

            /// <summary>
            /// Exports all the contacts from the provided segment
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentName">Name of your segment.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <param name="fileName">Name of your file.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(string segmentName, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None, string fileName = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentName", segmentName);
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                if (fileName != null) values.Add("fileName", fileName);
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/segment/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists all your available Segments
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="includeHistory">True: Include history of last 30 days. Otherwise, false.</param>
            /// <param name="from">From what date should the segment history be shown. In YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">To what date should the segment history be shown. In YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>List(ApiTypes.Segment)</returns>
            public static async Task<List<ApiTypes.Segment>> ListAsync(bool includeHistory = false, DateTime? from = null, DateTime? to = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (includeHistory != false) values.Add("includeHistory", includeHistory.ToString());
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<List<ApiTypes.Segment>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Segment>>("/segment/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists your available Segments using the provided names
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentNames">Names of segments you want to load. Will load all contacts if left empty or the 'All Contacts' name has been provided</param>
            /// <param name="includeHistory">True: Include history of last 30 days. Otherwise, false.</param>
            /// <param name="from">From what date should the segment history be shown. In YYYY-MM-DDThh:mm:ss format.</param>
            /// <param name="to">To what date should the segment history be shown. In YYYY-MM-DDThh:mm:ss format.</param>
            /// <returns>List(ApiTypes.Segment)</returns>
            public static async Task<List<ApiTypes.Segment>> LoadByNameAsync(IEnumerable<string> segmentNames, bool includeHistory = false, DateTime? from = null, DateTime? to = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentNames", string.Join(",", segmentNames));
                if (includeHistory != false) values.Add("includeHistory", includeHistory.ToString());
                if (from != null) values.Add("from", from.Value.ToString("M/d/yyyy h:mm:ss tt"));
                if (to != null) values.Add("to", to.Value.ToString("M/d/yyyy h:mm:ss tt"));
                ApiResponse<List<ApiTypes.Segment>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Segment>>("/segment/loadbyname", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Rename or change RULE for your segment
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="segmentName">Name of your segment.</param>
            /// <param name="newSegmentName">New name of your segment if you want to change it.</param>
            /// <param name="rule">Query used for filtering.</param>
            /// <returns>ApiTypes.Segment</returns>
            public static async Task<ApiTypes.Segment> UpdateAsync(string segmentName, string newSegmentName = null, string rule = null)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("segmentName", segmentName);
                if (newSegmentName != null) values.Add("newSegmentName", newSegmentName);
                if (rule != null) values.Add("rule", rule);
                ApiResponse<ApiTypes.Segment> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Segment>("/segment/update", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region SMS functions
        /// <summary>
        /// Managing texting to your clients.
        /// </summary>
        public static class SMS
        {
            /// <summary>
            /// Send a short SMS Message (maximum of 1600 characters) to any mobile phone.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="to">Mobile number you want to message. Can be any valid mobile number in E.164 format. To provide the country code you need to provide "+" before the number.  If your URL is not encoded then you need to replace the "+" with "%2B" instead.</param>
            /// <param name="body">Body of your message. The maximum body length is 160 characters.  If the message body is greater than 160 characters it is split into multiple messages and you are charged per message for the number of message required to send your length</param>
            public static async Task SendAsync(string to, string body)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("to", to);
                values.Add("body", body);
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/sms/send", values);
            }

        }
        #endregion


        #region Survey functions
        /// <summary>
        /// Methods to organize and get results of your surveys
        /// </summary>
        public static class Survey
        {
            /// <summary>
            /// Adds a new survey
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="survey">Json representation of a survey</param>
            /// <returns>ApiTypes.Survey</returns>
            public static async Task<ApiTypes.Survey> AddAsync(ApiTypes.Survey survey)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("survey", Newtonsoft.Json.JsonConvert.SerializeObject(survey));
                ApiResponse<ApiTypes.Survey> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Survey>("/survey/add", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Deletes the survey
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicSurveyID">Survey identifier</param>
            public static async Task DeleteAsync(Guid publicSurveyID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicSurveyID", publicSurveyID.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/survey/delete", values);
            }

            /// <summary>
            /// Export given survey's data to provided format
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicSurveyID">Survey identifier</param>
            /// <param name="fileName">Name of your file.</param>
            /// <param name="fileFormat">Format of the exported file</param>
            /// <param name="compressionFormat">FileResponse compression format. None or Zip.</param>
            /// <returns>ApiTypes.ExportLink</returns>
            public static async Task<ApiTypes.ExportLink> ExportAsync(Guid publicSurveyID, string fileName, ApiTypes.ExportFileFormats fileFormat = ApiTypes.ExportFileFormats.Csv, ApiTypes.CompressionFormat compressionFormat = ApiTypes.CompressionFormat.None)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicSurveyID", publicSurveyID.ToString());
                values.Add("fileName", fileName);
                if (fileFormat != ApiTypes.ExportFileFormats.Csv) values.Add("fileFormat", fileFormat.ToString());
                if (compressionFormat != ApiTypes.CompressionFormat.None) values.Add("compressionFormat", compressionFormat.ToString());
                ApiResponse<ApiTypes.ExportLink> apiResponse = await ApiUtilities.PostAsync<ApiTypes.ExportLink>("/survey/export", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Shows all your existing surveys
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <returns>List(ApiTypes.Survey)</returns>
            public static async Task<List<ApiTypes.Survey>> ListAsync()
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                ApiResponse<List<ApiTypes.Survey>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.Survey>>("/survey/list", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Get list of personal answers for the specific survey
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicSurveyID">Survey identifier</param>
            /// <returns>List(ApiTypes.SurveyResultInfo)</returns>
            public static async Task<List<ApiTypes.SurveyResultInfo>> LoadResponseListAsync(Guid publicSurveyID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicSurveyID", publicSurveyID.ToString());
                ApiResponse<List<ApiTypes.SurveyResultInfo>> apiResponse = await ApiUtilities.PostAsync<List<ApiTypes.SurveyResultInfo>>("/survey/loadresponselist", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Get general results of the specific survey
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="publicSurveyID">Survey identifier</param>
            /// <returns>ApiTypes.SurveyResultsSummaryInfo</returns>
            public static async Task<ApiTypes.SurveyResultsSummaryInfo> LoadResultsAsync(Guid publicSurveyID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("publicSurveyID", publicSurveyID.ToString());
                ApiResponse<ApiTypes.SurveyResultsSummaryInfo> apiResponse = await ApiUtilities.PostAsync<ApiTypes.SurveyResultsSummaryInfo>("/survey/loadresults", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Update the survey information
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="survey">Json representation of a survey</param>
            /// <returns>ApiTypes.Survey</returns>
            public static async Task<ApiTypes.Survey> UpdateAsync(ApiTypes.Survey survey)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("survey", Newtonsoft.Json.JsonConvert.SerializeObject(survey));
                ApiResponse<ApiTypes.Survey> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Survey>("/survey/update", values);
                return apiResponse.Data;
            }

        }
        #endregion


        #region Template functions
        /// <summary>
        /// Managing and editing templates of your emails
        /// </summary>
        public static class Template
        {
            /// <summary>
            /// Create new Template. Needs to be sent using POST method
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="name">Filename</param>
            /// <param name="subject">Default subject of email.</param>
            /// <param name="fromEmail">Default From: email address.</param>
            /// <param name="fromName">Default From: name.</param>
            /// <param name="templateType">0 for API connections</param>
            /// <param name="templateScope">Enum: 0 - private, 1 - public, 2 - mockup</param>
            /// <param name="bodyHtml">HTML code of email (needs escaping).</param>
            /// <param name="bodyText">Text body of email.</param>
            /// <param name="css">CSS style</param>
            /// <param name="originalTemplateID">ID number of original template.</param>
            /// <returns>int</returns>
            public static async Task<int> AddAsync(string name, string subject, string fromEmail, string fromName, ApiTypes.TemplateType templateType = ApiTypes.TemplateType.RawHTML, ApiTypes.TemplateScope templateScope = ApiTypes.TemplateScope.Private, string bodyHtml = null, string bodyText = null, string css = null, int originalTemplateID = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("name", name);
                values.Add("subject", subject);
                values.Add("fromEmail", fromEmail);
                values.Add("fromName", fromName);
                if (templateType != ApiTypes.TemplateType.RawHTML) values.Add("templateType", templateType.ToString());
                if (templateScope != ApiTypes.TemplateScope.Private) values.Add("templateScope", templateScope.ToString());
                if (bodyHtml != null) values.Add("bodyHtml", bodyHtml);
                if (bodyText != null) values.Add("bodyText", bodyText);
                if (css != null) values.Add("css", css);
                if (originalTemplateID != 0) values.Add("originalTemplateID", originalTemplateID.ToString());
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/template/add", null, values);
                ApiResponse<int> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<int>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
                return apiRet.Data;
            }

            /// <summary>
            /// Check if template is used by campaign.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <returns>bool</returns>
            public static async Task<bool> CheckUsageAsync(int templateID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                ApiResponse<bool> apiResponse = await ApiUtilities.PostAsync<bool>("/template/checkusage", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Copy Selected Template
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <param name="name">Filename</param>
            /// <param name="subject">Default subject of email.</param>
            /// <param name="fromEmail">Default From: email address.</param>
            /// <param name="fromName">Default From: name.</param>
            /// <returns>ApiTypes.Template</returns>
            public static async Task<ApiTypes.Template> CopyAsync(int templateID, string name, string subject, string fromEmail, string fromName)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                values.Add("name", name);
                values.Add("subject", subject);
                values.Add("fromEmail", fromEmail);
                values.Add("fromName", fromName);
                ApiResponse<ApiTypes.Template> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Template>("/template/copy", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Delete template with the specified ID
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            public static async Task DeleteAsync(int templateID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/template/delete", values);
            }

            /// <summary>
            /// Search for references to images and replaces them with base64 code.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <returns>string</returns>
            public static async Task<string> GetEmbeddedHtmlAsync(int templateID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/template/getembeddedhtml", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Lists your templates
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="limit">Maximum of loaded items.</param>
            /// <param name="offset">How many items should be loaded ahead.</param>
            /// <returns>ApiTypes.TemplateList</returns>
            public static async Task<ApiTypes.TemplateList> GetListAsync(int limit = 500, int offset = 0)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                if (limit != 500) values.Add("limit", limit.ToString());
                if (offset != 0) values.Add("offset", offset.ToString());
                ApiResponse<ApiTypes.TemplateList> apiResponse = await ApiUtilities.PostAsync<ApiTypes.TemplateList>("/template/getlist", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Load template with content
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <param name="ispublic"></param>
            /// <returns>ApiTypes.Template</returns>
            public static async Task<ApiTypes.Template> LoadTemplateAsync(int templateID, bool ispublic = false)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                if (ispublic != false) values.Add("ispublic", ispublic.ToString());
                ApiResponse<ApiTypes.Template> apiResponse = await ApiUtilities.PostAsync<ApiTypes.Template>("/template/loadtemplate", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Removes previously generated screenshot of template
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            public static async Task RemoveScreenshotAsync(int templateID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                ApiResponse<VoidApiResponse> apiResponse = await ApiUtilities.PostAsync<VoidApiResponse>("/template/removescreenshot", values);
            }

            /// <summary>
            /// Saves screenshot of chosen Template
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="base64Image">Image, base64 coded.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <returns>string</returns>
            public static async Task<string> SaveScreenshotAsync(string base64Image, int templateID)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("base64Image", base64Image);
                values.Add("templateID", templateID.ToString());
                ApiResponse<string> apiResponse = await ApiUtilities.PostAsync<string>("/template/savescreenshot", values);
                return apiResponse.Data;
            }

            /// <summary>
            /// Update existing template, overwriting existing data. Needs to be sent using POST method.
            /// </summary>
            /// <param name="apikey">ApiKey that gives you access to our SMTP and HTTP API's.</param>
            /// <param name="templateID">ID number of template.</param>
            /// <param name="templateScope">Enum: 0 - private, 1 - public, 2 - mockup</param>
            /// <param name="name">Filename</param>
            /// <param name="subject">Default subject of email.</param>
            /// <param name="fromEmail">Default From: email address.</param>
            /// <param name="fromName">Default From: name.</param>
            /// <param name="bodyHtml">HTML code of email (needs escaping).</param>
            /// <param name="bodyText">Text body of email.</param>
            /// <param name="css">CSS style</param>
            /// <param name="removeScreenshot"></param>
            public static async Task UpdateAsync(int templateID, ApiTypes.TemplateScope templateScope = ApiTypes.TemplateScope.Private, string name = null, string subject = null, string fromEmail = null, string fromName = null, string bodyHtml = null, string bodyText = null, string css = null, bool removeScreenshot = true)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("apikey", Api.ApiKey);
                values.Add("templateID", templateID.ToString());
                if (templateScope != ApiTypes.TemplateScope.Private) values.Add("templateScope", templateScope.ToString());
                if (name != null) values.Add("name", name);
                if (subject != null) values.Add("subject", subject);
                if (fromEmail != null) values.Add("fromEmail", fromEmail);
                if (fromName != null) values.Add("fromName", fromName);
                if (bodyHtml != null) values.Add("bodyHtml", bodyHtml);
                if (bodyText != null) values.Add("bodyText", bodyText);
                if (css != null) values.Add("css", css);
                if (removeScreenshot != true) values.Add("removeScreenshot", removeScreenshot.ToString());
                byte[] apiResponse = await ApiUtilities.HttpPostFileAsync("/template/update", null, values);
                ApiResponse<VoidApiResponse> apiRet = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<VoidApiResponse>>(Encoding.UTF8.GetString(apiResponse));
                if (!apiRet.success) throw new Exception(apiRet.error);
            }

        }
        #endregion


    }
    #region Api Types
    public static class ApiTypes
    {
        /// <summary>
        /// File response from the server
        /// </summary>
        public class FileData
        {
            /// <summary>
            /// File content
            /// </summary>
            public byte[] Content { get; set; }

            /// <summary>
            /// MIME content type, optional for uploads
            /// </summary>
            public string ContentType { get; set; }

            /// <summary>
            /// Name of the file this class contains
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Saves this file to given destination
            /// </summary>
            /// <param name="path">Path string exluding file name</param>
            public void SaveToDirectory(string path)
            {
                System.IO.File.WriteAllBytes(Path.Combine(path, FileName), Content);
            }

            /// <summary>
            /// Saves this file to given destination
            /// </summary>
            /// <param name="pathWithFileName">Path string including file name</param>
            public void SaveTo(string pathWithFileName)
            {
                System.IO.File.WriteAllBytes(pathWithFileName, Content);
            }

            /// <summary>
            /// Reads a file to this class instance
            /// </summary>
            /// <param name="pathWithFileName">Path string including file name</param>
            public void ReadFrom(string pathWithFileName)
            {
                Content = System.IO.File.ReadAllBytes(pathWithFileName);
                FileName = Path.GetFileName(pathWithFileName);
                ContentType = MimeMapping.GetMimeMapping(FileName);
            }

            /// <summary>
            /// Creates a new FileData instance from a file
            /// </summary>
            /// <param name="pathWithFileName">Path string including file name</param>
            /// <returns></returns>
            public static FileData CreateFromFile(string pathWithFileName)
            {
                FileData fileData = new FileData();
                fileData.ReadFrom(pathWithFileName);
                return fileData;
            }
        }

#pragma warning disable 0649

        /// <summary>
        /// </summary>
        public enum AccessLevel : Int64
        {
            /// <summary>
            /// </summary>
            None = 0,

            /// <summary>
            /// </summary>
            ViewAccount = 1,

            /// <summary>
            /// </summary>
            ViewContacts = 2,

            /// <summary>
            /// </summary>
            ViewForms = 4,

            /// <summary>
            /// </summary>
            ViewTemplates = 8,

            /// <summary>
            /// </summary>
            ViewCampaigns = 16,

            /// <summary>
            /// </summary>
            ViewChannels = 32,

            /// <summary>
            /// </summary>
            ViewJourneys = 64,

            /// <summary>
            /// </summary>
            ViewSurveys = 128,

            /// <summary>
            /// </summary>
            ViewSettings = 256,

            /// <summary>
            /// </summary>
            ViewBilling = 512,

            /// <summary>
            /// </summary>
            ViewSubAccounts = 1024,

            /// <summary>
            /// </summary>
            ViewUsers = 2048,

            /// <summary>
            /// </summary>
            ViewFiles = 4096,

            /// <summary>
            /// </summary>
            ViewReports = 8192,

            /// <summary>
            /// </summary>
            ModifyAccount = 16384,

            /// <summary>
            /// </summary>
            ModifyContacts = 32768,

            /// <summary>
            /// </summary>
            ModifyForms = 65536,

            /// <summary>
            /// </summary>
            ModifyTemplates = 131072,

            /// <summary>
            /// </summary>
            ModifyCampaigns = 262144,

            /// <summary>
            /// </summary>
            ModifyChannels = 524288,

            /// <summary>
            /// </summary>
            ModifyJourneys = 1048576,

            /// <summary>
            /// </summary>
            ModifySurveys = 2097152,

            /// <summary>
            /// </summary>
            ModifyFiles = 4194304,

            /// <summary>
            /// </summary>
            Export = 8388608,

            /// <summary>
            /// </summary>
            SendSmtp = 16777216,

            /// <summary>
            /// </summary>
            SendSMS = 33554432,

            /// <summary>
            /// </summary>
            ModifySettings = 67108864,

            /// <summary>
            /// </summary>
            ModifyBilling = 134217728,

            /// <summary>
            /// </summary>
            ModifyProfile = 268435456,

            /// <summary>
            /// </summary>
            ModifySubAccounts = 536870912,

            /// <summary>
            /// </summary>
            ModifyUsers = 1073741824,

            /// <summary>
            /// </summary>
            Security = 2147483648,

            /// <summary>
            /// </summary>
            ModifyLanguage = 4294967296,

            /// <summary>
            /// </summary>
            ModifySupport = 8589934592,

            /// <summary>
            /// </summary>
            ViewSupport = 8589934592,

            /// <summary>
            /// </summary>
            SendHttp = 17179869184,

        }

        /// <summary>
        /// </summary>
        public class AccessToken
        {
        }

        /// <summary>
        /// Detailed information about your account
        /// </summary>
        public class Account
        {
            /// <summary>
            /// Code used for tax purposes.
            /// </summary>
            public string TaxCode { get; set; }

            /// <summary>
            /// Public key for limited access to your account such as contact/add so you can use it safely on public websites.
            /// </summary>
            public string PublicAccountID { get; set; }

            /// <summary>
            /// ApiKey that gives you access to our SMTP and HTTP API's.
            /// </summary>
            public string ApiKey { get; set; }

            /// <summary>
            /// Second ApiKey that gives you access to our SMTP and HTTP API's.  Used mainly for changing ApiKeys without disrupting services.
            /// </summary>
            public string ApiKey2 { get; set; }

            /// <summary>
            /// True, if account is a subaccount. Otherwise, false
            /// </summary>
            public bool IsSub { get; set; }

            /// <summary>
            /// The number of subaccounts this account has.
            /// </summary>
            public long SubAccountsCount { get; set; }

            /// <summary>
            /// Number of status: 1 - Active
            /// </summary>
            public int StatusNumber { get; set; }

            /// <summary>
            /// Account status: Active
            /// </summary>
            public string StatusFormatted { get; set; }

            /// <summary>
            /// URL form for payments.
            /// </summary>
            public string PaymentFormUrl { get; set; }

            /// <summary>
            /// URL to your logo image.
            /// </summary>
            public string LogoUrl { get; set; }

            /// <summary>
            /// HTTP address of your website.
            /// </summary>
            public string Website { get; set; }

            /// <summary>
            /// True: Turn on or off ability to send mails under your brand. Otherwise, false
            /// </summary>
            public bool EnablePrivateBranding { get; set; }

            /// <summary>
            /// Address to your support.
            /// </summary>
            public string SupportLink { get; set; }

            /// <summary>
            /// Subdomain for your rebranded service
            /// </summary>
            public string PrivateBrandingUrl { get; set; }

            /// <summary>
            /// First name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Company name.
            /// </summary>
            public string Company { get; set; }

            /// <summary>
            /// First line of address.
            /// </summary>
            public string Address1 { get; set; }

            /// <summary>
            /// Second line of address.
            /// </summary>
            public string Address2 { get; set; }

            /// <summary>
            /// City.
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// State or province.
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// Zip/postal code.
            /// </summary>
            public string Zip { get; set; }

            /// <summary>
            /// Numeric ID of country. A file with the list of countries is available <a href="http://api.elasticemail.com/public/countries"><b>here</b></a>
            /// </summary>
            public int? CountryID { get; set; }

            /// <summary>
            /// Phone number
            /// </summary>
            public string Phone { get; set; }

            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// URL for affiliating.
            /// </summary>
            public string AffiliateLink { get; set; }

            /// <summary>
            /// Numeric reputation
            /// </summary>
            public double Reputation { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public long TotalEmailsSent { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public long? MonthlyEmailsSent { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public decimal Credit { get; set; }

            /// <summary>
            /// Amount of email credits
            /// </summary>
            public int EmailCredits { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public decimal PricePerEmail { get; set; }

            /// <summary>
            /// Why your clients are receiving your emails.
            /// </summary>
            public string DeliveryReason { get; set; }

            /// <summary>
            /// URL for making payments.
            /// </summary>
            public string AccountPaymentUrl { get; set; }

            /// <summary>
            /// Address of SMTP server.
            /// </summary>
            public string Smtp { get; set; }

            /// <summary>
            /// Address of alternative SMTP server.
            /// </summary>
            public string SmtpAlternative { get; set; }

            /// <summary>
            /// Status of automatic payments configuration.
            /// </summary>
            public string AutoCreditStatus { get; set; }

            /// <summary>
            /// When AutoCreditStatus is Enabled, the credit level that triggers the credit to be recharged.
            /// </summary>
            public decimal AutoCreditLevel { get; set; }

            /// <summary>
            /// When AutoCreditStatus is Enabled, the amount of credit to be recharged.
            /// </summary>
            public decimal AutoCreditAmount { get; set; }

            /// <summary>
            /// Amount of emails account can send daily
            /// </summary>
            public int DailySendLimit { get; set; }

            /// <summary>
            /// Creation date.
            /// </summary>
            public DateTime DateCreated { get; set; }

            /// <summary>
            /// True, if you have enabled link tracking. Otherwise, false
            /// </summary>
            public bool LinkTracking { get; set; }

            /// <summary>
            /// Type of content encoding
            /// </summary>
            public string ContentTransferEncoding { get; set; }

            /// <summary>
            /// Amount of Litmus credits
            /// </summary>
            public decimal LitmusCredits { get; set; }

            /// <summary>
            /// Enable contact delivery and optimization tools on your Account.
            /// </summary>
            public bool EnableContactFeatures { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool NeedsSMSVerification { get; set; }

            /// <summary>
            /// </summary>
            public bool DisableGlobalContacts { get; set; }

        }

        /// <summary>
        /// Basic overview of your account
        /// </summary>
        public class AccountOverview
        {
            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public long TotalEmailsSent { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public decimal Credit { get; set; }

            /// <summary>
            /// Cost of 1000 emails
            /// </summary>
            public decimal CostPerThousand { get; set; }

            /// <summary>
            /// Number of messages in progress
            /// </summary>
            public long InProgressCount { get; set; }

            /// <summary>
            /// Number of contacts currently with blocked status of Unsubscribed, Complaint, Bounced or InActive
            /// </summary>
            public long BlockedContactsCount { get; set; }

            /// <summary>
            /// Numeric reputation
            /// </summary>
            public double Reputation { get; set; }

            /// <summary>
            /// Number of contacts
            /// </summary>
            public long ContactCount { get; set; }

            /// <summary>
            /// Number of created campaigns
            /// </summary>
            public long CampaignCount { get; set; }

            /// <summary>
            /// Number of available templates
            /// </summary>
            public long TemplateCount { get; set; }

            /// <summary>
            /// Number of created subaccounts
            /// </summary>
            public long SubAccountCount { get; set; }

            /// <summary>
            /// Number of active referrals
            /// </summary>
            public long ReferralCount { get; set; }

        }

        /// <summary>
        /// Account's ready to send e-mails Status
        /// </summary>
        public enum AccountSendStatus : Int32
        {
            /// <summary>
            /// Account doesn't have enough credits
            /// </summary>
            NotEnoughCredits = 1,

            /// <summary>
            /// Account can send e-mails but only without the attachments
            /// </summary>
            CanSendEmailsNoAttachments = 2,

            /// <summary>
            /// Account has exceeded his daily send limit
            /// </summary>
            DailySendLimitExceeded = 3,

            /// <summary>
            /// Account is ready to send e-mails
            /// </summary>
            CanSendEmails = 4,

        }

        /// <summary>
        /// Lists advanced sending options of your account.
        /// </summary>
        public class AdvancedOptions
        {
            /// <summary>
            /// True, if you want to track clicks. Otherwise, false
            /// </summary>
            public bool EnableClickTracking { get; set; }

            /// <summary>
            /// True, if you want to track by link tracking. Otherwise, false
            /// </summary>
            public bool EnableLinkClickTracking { get; set; }

            /// <summary>
            /// True, if you want to use template scripting in your emails {{}}. Otherwise, false
            /// </summary>
            public bool EnableTemplateScripting { get; set; }

            /// <summary>
            /// True, if text BODY of message should be created automatically. Otherwise, false
            /// </summary>
            public bool AutoTextFormat { get; set; }

            /// <summary>
            /// True, if you want bounce notifications returned. Otherwise, false
            /// </summary>
            public bool EmailNotificationForError { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for sent email. Otherwise, false
            /// </summary>
            public bool WebNotificationForSent { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for opened email. Otherwise, false
            /// </summary>
            public bool WebNotificationForOpened { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for clicked email. Otherwise, false
            /// </summary>
            public bool WebNotificationForClicked { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for unsubscribed email. Otherwise, false
            /// </summary>
            public bool WebnotificationForUnsubscribed { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for complaint email. Otherwise, false
            /// </summary>
            public bool WebNotificationForAbuse { get; set; }

            /// <summary>
            /// True, if you want to send web notifications for bounced email. Otherwise, false
            /// </summary>
            public bool WebNotificationForError { get; set; }

            /// <summary>
            /// True, if you want to receive notifications for each type only once per email. Otherwise, false
            /// </summary>
            public bool WebNotificationNotifyOncePerEmail { get; set; }

            /// <summary>
            /// True, if you want to receive low credit email notifications. Otherwise, false
            /// </summary>
            public bool LowCreditNotification { get; set; }

            /// <summary>
            /// True, if you want inbound email to only process contacts from your account. Otherwise, false
            /// </summary>
            public bool InboundContactsOnly { get; set; }

            /// <summary>
            /// True, if this account is a sub-account. Otherwise, false
            /// </summary>
            public bool IsSubAccount { get; set; }

            /// <summary>
            /// True, if this account resells Elastic Email. Otherwise, false.
            /// </summary>
            public bool IsOwnedByReseller { get; set; }

            /// <summary>
            /// True, if you want to enable list-unsubscribe header. Otherwise, false
            /// </summary>
            public bool EnableUnsubscribeHeader { get; set; }

            /// <summary>
            /// True, if you want to display your labels on your unsubscribe form. Otherwise, false
            /// </summary>
            public bool ManageSubscriptions { get; set; }

            /// <summary>
            /// True, if you want to only display labels that the contact is subscribed to on your unsubscribe form. Otherwise, false
            /// </summary>
            public bool ManageSubscribedOnly { get; set; }

            /// <summary>
            /// True, if you want to display an option for the contact to opt into transactional email only on your unsubscribe form. Otherwise, false
            /// </summary>
            public bool TransactionalOnUnsubscribe { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool ConsentTrackingOnUnsubscribe { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string PreviewMessageID { get; set; }

            /// <summary>
            /// True, if you want to apply custom headers to your emails. Otherwise, false
            /// </summary>
            public bool AllowCustomHeaders { get; set; }

            /// <summary>
            /// Email address to send a copy of all email to.
            /// </summary>
            public string BccEmail { get; set; }

            /// <summary>
            /// Type of content encoding
            /// </summary>
            public string ContentTransferEncoding { get; set; }

            /// <summary>
            /// True, if you want to receive bounce email notifications. Otherwise, false
            /// </summary>
            public string EmailNotification { get; set; }

            /// <summary>
            /// Email addresses to send a copy of all notifications from our system. Separated by semicolon
            /// </summary>
            public string NotificationsEmails { get; set; }

            /// <summary>
            /// Emails, separated by semicolon, to which the notification about contact unsubscribing should be sent to
            /// </summary>
            public string UnsubscribeNotificationEmails { get; set; }

            /// <summary>
            /// URL address to receive web notifications to parse and process.
            /// </summary>
            public string WebNotificationUrl { get; set; }

            /// <summary>
            /// URL used for tracking action of inbound emails
            /// </summary>
            public string HubCallbackUrl { get; set; }

            /// <summary>
            /// Domain you use as your inbound domain
            /// </summary>
            public string InboundDomain { get; set; }

            /// <summary>
            /// True, if account has tooltips active. Otherwise, false
            /// </summary>
            public bool EnableUITooltips { get; set; }

            /// <summary>
            /// True, if you want to use Contact Delivery Tools.  Otherwise, false
            /// </summary>
            public bool EnableContactFeatures { get; set; }

            /// <summary>
            /// URL to your logo image.
            /// </summary>
            public string LogoUrl { get; set; }

            /// <summary>
            /// (0 means this functionality is NOT enabled) Score, depending on the number of times you have sent to a recipient, at which the given recipient should be moved to the Stale status
            /// </summary>
            public int StaleContactScore { get; set; }

            /// <summary>
            /// (0 means this functionality is NOT enabled) Number of days of inactivity for a contact after which the given recipient should be moved to the Stale status
            /// </summary>
            public int StaleContactInactiveDays { get; set; }

            /// <summary>
            /// Why your clients are receiving your emails.
            /// </summary>
            public string DeliveryReason { get; set; }

        }

        /// <summary>
        /// Blocked Contact - Contact returning Hard Bounces
        /// </summary>
        public class BlockedContact
        {
            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// RFC error message
            /// </summary>
            public string FriendlyErrorMessage { get; set; }

            /// <summary>
            /// Last change date
            /// </summary>
            public string DateUpdated { get; set; }

        }

        /// <summary>
        /// Summary of bounced categories, based on specified date range.
        /// </summary>
        public class BouncedCategorySummary
        {
            /// <summary>
            /// Number of messages marked as SPAM
            /// </summary>
            public long Spam { get; set; }

            /// <summary>
            /// Number of blacklisted messages
            /// </summary>
            public long BlackListed { get; set; }

            /// <summary>
            /// Number of messages flagged with 'No Mailbox'
            /// </summary>
            public long NoMailbox { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Grey Listed'
            /// </summary>
            public long GreyListed { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Throttled'
            /// </summary>
            public long Throttled { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Timeout'
            /// </summary>
            public long Timeout { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Connection Problem'
            /// </summary>
            public long ConnectionProblem { get; set; }

            /// <summary>
            /// Number of messages flagged with 'SPF Problem'
            /// </summary>
            public long SpfProblem { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Account Problem'
            /// </summary>
            public long AccountProblem { get; set; }

            /// <summary>
            /// Number of messages flagged with 'DNS Problem'
            /// </summary>
            public long DnsProblem { get; set; }

            /// <summary>
            /// Number of messages flagged with 'WhiteListing Problem'
            /// </summary>
            public long WhitelistingProblem { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Code Error'
            /// </summary>
            public long CodeError { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Not Delivered'
            /// </summary>
            public long NotDelivered { get; set; }

            /// <summary>
            /// Number of manually cancelled messages
            /// </summary>
            public long ManualCancel { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Connection terminated'
            /// </summary>
            public long ConnectionTerminated { get; set; }

        }

        /// <summary>
        /// Campaign
        /// </summary>
        public class Campaign
        {
            /// <summary>
            /// ID number of selected Channel.
            /// </summary>
            public int? ChannelID { get; set; }

            /// <summary>
            /// Campaign's name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Name of campaign's status
            /// </summary>
            public ApiTypes.CampaignStatus Status { get; set; }

            /// <summary>
            /// List of Segment and List IDs, preceded with 'l' for Lists and 's' for Segments, comma separated
            /// </summary>
            public string[] Targets { get; set; }

            /// <summary>
            /// Number of event, triggering mail sending
            /// </summary>
            public ApiTypes.CampaignTriggerType TriggerType { get; set; }

            /// <summary>
            /// Date of triggered send
            /// </summary>
            public DateTime? TriggerDate { get; set; }

            /// <summary>
            /// How far into the future should the campaign be sent, in minutes
            /// </summary>
            public double TriggerDelay { get; set; }

            /// <summary>
            /// When your next automatic mail will be sent, in minutes
            /// </summary>
            public double TriggerFrequency { get; set; }

            /// <summary>
            /// How many times should the campaign be sent
            /// </summary>
            public int TriggerCount { get; set; }

            /// <summary>
            /// ID number of transaction
            /// </summary>
            public int TriggerChannelID { get; set; }

            /// <summary>
            /// Data for filtering event campaigns such as specific link addresses.
            /// </summary>
            public string TriggerData { get; set; }

            /// <summary>
            /// What should be checked for choosing the winner: opens or clicks
            /// </summary>
            public ApiTypes.SplitOptimization SplitOptimization { get; set; }

            /// <summary>
            /// Number of minutes between sends during optimization period
            /// </summary>
            public int SplitOptimizationMinutes { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int TimingOption { get; set; }

            /// <summary>
            /// Should the opens be tracked? If no value has been provided, account's default setting will be used.
            /// </summary>
            public bool? TrackOpens { get; set; }

            /// <summary>
            /// Should the clicks be tracked? If no value has been provided, account's default setting will be used.
            /// </summary>
            public bool? TrackClicks { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.CampaignTemplate> CampaignTemplates { get; set; }

        }

        /// <summary>
        /// Channel
        /// </summary>
        public class CampaignChannel
        {
            /// <summary>
            /// ID number of selected Channel.
            /// </summary>
            public int ChannelID { get; set; }

            /// <summary>
            /// Filename
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// True, if you are sending a campaign. Otherwise, false.
            /// </summary>
            public bool IsCampaign { get; set; }

            /// <summary>
            /// Name of your custom IP Pool to be used in the sending process
            /// </summary>
            public string PoolName { get; set; }

            /// <summary>
            /// Date of creation in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// Name of campaign's status
            /// </summary>
            public ApiTypes.CampaignStatus Status { get; set; }

            /// <summary>
            /// Date of last activity on account
            /// </summary>
            public DateTime? LastActivity { get; set; }

            /// <summary>
            /// Datetime of last action done on campaign.
            /// </summary>
            public DateTime? LastProcessed { get; set; }

            /// <summary>
            /// Id number of parent channel
            /// </summary>
            public int ParentChannelID { get; set; }

            /// <summary>
            /// List of Segment and List IDs, preceded with 'l' for Lists and 's' for Segments, comma separated
            /// </summary>
            public string[] Targets { get; set; }

            /// <summary>
            /// Number of event, triggering mail sending
            /// </summary>
            public ApiTypes.CampaignTriggerType TriggerType { get; set; }

            /// <summary>
            /// Date of triggered send
            /// </summary>
            public DateTime? TriggerDate { get; set; }

            /// <summary>
            /// How far into the future should the campaign be sent, in minutes
            /// </summary>
            public double TriggerDelay { get; set; }

            /// <summary>
            /// When your next automatic mail will be sent, in minutes
            /// </summary>
            public double TriggerFrequency { get; set; }

            /// <summary>
            /// How many times should the campaign be sent
            /// </summary>
            public int TriggerCount { get; set; }

            /// <summary>
            /// ID number of transaction
            /// </summary>
            public int TriggerChannelID { get; set; }

            /// <summary>
            /// Data for filtering event campaigns such as specific link addresses.
            /// </summary>
            public string TriggerData { get; set; }

            /// <summary>
            /// What should be checked for choosing the winner: opens or clicks
            /// </summary>
            public ApiTypes.SplitOptimization SplitOptimization { get; set; }

            /// <summary>
            /// Number of minutes between sends during optimization period
            /// </summary>
            public int SplitOptimizationMinutes { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int TimingOption { get; set; }

            /// <summary>
            /// ID number of template.
            /// </summary>
            public int? TemplateID { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string TemplateSubject { get; set; }

            /// <summary>
            /// Default From: email address.
            /// </summary>
            public string TemplateFromEmail { get; set; }

            /// <summary>
            /// Default From: name.
            /// </summary>
            public string TemplateFromName { get; set; }

            /// <summary>
            /// Default Reply: email address.
            /// </summary>
            public string TemplateReplyEmail { get; set; }

            /// <summary>
            /// Default Reply: name.
            /// </summary>
            public string TemplateReplyName { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public int ClickedCount { get; set; }

            /// <summary>
            /// Total emails opened.
            /// </summary>
            public int OpenedCount { get; set; }

            /// <summary>
            /// Overall number of recipients
            /// </summary>
            public int RecipientCount { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int SentCount { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public int UnsubscribedCount { get; set; }

            /// <summary>
            /// Abuses - mails sent to user without their consent
            /// </summary>
            public int FailedAbuse { get; set; }

            /// <summary>
            /// List of CampaignTemplate for sending A-X split testing.
            /// </summary>
            public List<ApiTypes.CampaignChannel> TemplateChannels { get; set; }

            /// <summary>
            /// Should the opens be tracked? If no value has been provided, account's default setting will be used.
            /// </summary>
            public bool? TrackOpens { get; set; }

            /// <summary>
            /// Should the clicks be tracked? If no value has been provided, account's default setting will be used.
            /// </summary>
            public bool? TrackClicks { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum CampaignStatus : Int32
        {
            /// <summary>
            /// Campaign is logically deleted and not returned by API or interface calls.
            /// </summary>
            Deleted = -1,

            /// <summary>
            /// Campaign is curently active and available.
            /// </summary>
            Active = 0,

            /// <summary>
            /// Campaign is currently being processed for delivery.
            /// </summary>
            Processing = 1,

            /// <summary>
            /// Campaign is currently sending.
            /// </summary>
            Sending = 2,

            /// <summary>
            /// Campaign has completed sending.
            /// </summary>
            Completed = 3,

            /// <summary>
            /// Campaign is currently paused and not sending.
            /// </summary>
            Paused = 4,

            /// <summary>
            /// Campaign has been cancelled during delivery.
            /// </summary>
            Cancelled = 5,

            /// <summary>
            /// Campaign is save as draft and not processing.
            /// </summary>
            Draft = 6,

        }

        /// <summary>
        /// 
        /// </summary>
        public class CampaignTemplate
        {
            /// <summary>
            /// ID number of selected Channel.
            /// </summary>
            public int? ChannelID { get; set; }

            /// <summary>
            /// Name of campaign's status
            /// </summary>
            public ApiTypes.CampaignStatus Status { get; set; }

            /// <summary>
            /// Name of your custom IP Pool to be used in the sending process
            /// </summary>
            public string PoolName { get; set; }

            /// <summary>
            /// ID number of template.
            /// </summary>
            public int? TemplateID { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string TemplateSubject { get; set; }

            /// <summary>
            /// Default From: email address.
            /// </summary>
            public string TemplateFromEmail { get; set; }

            /// <summary>
            /// Default From: name.
            /// </summary>
            public string TemplateFromName { get; set; }

            /// <summary>
            /// Default Reply: email address.
            /// </summary>
            public string TemplateReplyEmail { get; set; }

            /// <summary>
            /// Default Reply: name.
            /// </summary>
            public string TemplateReplyName { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum CampaignTriggerType : Int32
        {
            /// <summary>
            /// 
            /// </summary>
            SendNow = 1,

            /// <summary>
            /// 
            /// </summary>
            FutureScheduled = 2,

            /// <summary>
            /// 
            /// </summary>
            OnAdd = 3,

            /// <summary>
            /// 
            /// </summary>
            OnOpen = 4,

            /// <summary>
            /// 
            /// </summary>
            OnClick = 5,

        }

        /// <summary>
        /// </summary>
        public enum CertificateValidationStatus : Int32
        {
            /// <summary>
            /// </summary>
            ErrorOccured = -2,

            /// <summary>
            /// </summary>
            CertNotSet = 0,

            /// <summary>
            /// </summary>
            Valid = 1,

            /// <summary>
            /// </summary>
            NotValid = 2,

        }

        /// <summary>
        /// SMTP and HTTP API channel for grouping email delivery
        /// </summary>
        public class Channel
        {
            /// <summary>
            /// Descriptive name of the channel.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The date the channel was added to your account.
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// The date the channel was last sent through.
            /// </summary>
            public DateTime? LastActivity { get; set; }

            /// <summary>
            /// The number of email jobs this channel has been used with.
            /// </summary>
            public int JobCount { get; set; }

            /// <summary>
            /// The number of emails that have been clicked within this channel.
            /// </summary>
            public int ClickedCount { get; set; }

            /// <summary>
            /// The number of emails that have been opened within this channel.
            /// </summary>
            public int OpenedCount { get; set; }

            /// <summary>
            /// The number of emails attempted to be sent within this channel.
            /// </summary>
            public int RecipientCount { get; set; }

            /// <summary>
            /// The number of emails that have been sent within this channel.
            /// </summary>
            public int SentCount { get; set; }

            /// <summary>
            /// The number of emails that have been bounced within this channel.
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// The number of emails that have been unsubscribed within this channel.
            /// </summary>
            public int UnsubscribedCount { get; set; }

            /// <summary>
            /// The number of emails that have been marked as abuse or complaint within this channel.
            /// </summary>
            public int FailedAbuse { get; set; }

            /// <summary>
            /// The total cost for emails/attachments within this channel.
            /// </summary>
            public decimal Cost { get; set; }

        }

        /// <summary>
        /// FileResponse compression format
        /// </summary>
        public enum CompressionFormat : Int32
        {
            /// <summary>
            /// No compression
            /// </summary>
            None = 0,

            /// <summary>
            /// Zip compression
            /// </summary>
            Zip = 1,

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ConsentTracking : Int32
        {
            /// <summary>
            /// 
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// 
            /// </summary>
            Allow = 1,

            /// <summary>
            /// 
            /// </summary>
            Deny = 2,

        }

        /// <summary>
        /// Contact
        /// </summary>
        public class Contact
        {
            /// <summary>
            /// 
            /// </summary>
            public int ContactScore { get; set; }

            /// <summary>
            /// Date of creation in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// First name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Name of status: Active, Engaged, Inactive, Abuse, Bounced, Unsubscribed.
            /// </summary>
            public ApiTypes.ContactStatus Status { get; set; }

            /// <summary>
            /// RFC Error code
            /// </summary>
            public int? BouncedErrorCode { get; set; }

            /// <summary>
            /// RFC error message
            /// </summary>
            public string BouncedErrorMessage { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int TotalSent { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int TotalFailed { get; set; }

            /// <summary>
            /// Total emails opened.
            /// </summary>
            public int TotalOpened { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public int TotalClicked { get; set; }

            /// <summary>
            /// Date of first failed message
            /// </summary>
            public DateTime? FirstFailedDate { get; set; }

            /// <summary>
            /// Number of fails in sending to this Contact
            /// </summary>
            public int LastFailedCount { get; set; }

            /// <summary>
            /// Last change date
            /// </summary>
            public DateTime DateUpdated { get; set; }

            /// <summary>
            /// Source of URL of payment
            /// </summary>
            public ApiTypes.ContactSource Source { get; set; }

            /// <summary>
            /// RFC Error code
            /// </summary>
            public int? ErrorCode { get; set; }

            /// <summary>
            /// RFC error message
            /// </summary>
            public string FriendlyErrorMessage { get; set; }

            /// <summary>
            /// IP address
            /// </summary>
            public string CreatedFromIP { get; set; }

            /// <summary>
            /// IP address of consent to send this contact(s) your email. If not provided your current public IP address is used for consent.
            /// </summary>
            public string ConsentIP { get; set; }

            /// <summary>
            /// Date of consent to send this contact(s) your email. If not provided current date is used for consent.
            /// </summary>
            public DateTime? ConsentDate { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public ApiTypes.ConsentTracking ConsentTracking { get; set; }

            /// <summary>
            /// Unsubscribed date in YYYY-MM-DD format
            /// </summary>
            public DateTime? UnsubscribedDate { get; set; }

            /// <summary>
            /// Free form field of notes
            /// </summary>
            public string Notes { get; set; }

            /// <summary>
            /// Website of contact
            /// </summary>
            public string WebsiteUrl { get; set; }

            /// <summary>
            /// Date this contact last opened an email
            /// </summary>
            public DateTime? LastOpened { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime? LastClicked { get; set; }

            /// <summary>
            /// Custom contact field like firstname, lastname, city etc. JSON serialized text like { "city":"london" }
            /// </summary>
            public Dictionary<string, string> CustomFields { get; set; }

        }

        /// <summary>
        /// Collection of lists and segments
        /// </summary>
        public class ContactCollection
        {
            /// <summary>
            /// Lists which contain the requested contact
            /// </summary>
            public List<ApiTypes.ContactContainer> Lists { get; set; }

            /// <summary>
            /// Segments which contain the requested contact
            /// </summary>
            public List<ApiTypes.ContactContainer> Segments { get; set; }

        }

        /// <summary>
        /// List's or segment's short info
        /// </summary>
        public class ContactContainer
        {
            /// <summary>
            /// ID of the list/segment
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Name of the list/segment
            /// </summary>
            public string Name { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum ContactHistEventType : Int32
        {
            /// <summary>
            /// Contact opened an e-mail
            /// </summary>
            Opened = 2,

            /// <summary>
            /// Contact clicked an e-mail
            /// </summary>
            Clicked = 3,

            /// <summary>
            /// E-mail sent to the contact bounced
            /// </summary>
            Bounced = 10,

            /// <summary>
            /// Contact unsubscribed
            /// </summary>
            Unsubscribed = 11,

            /// <summary>
            /// Contact complained to an e-mail
            /// </summary>
            Complained = 12,

            /// <summary>
            /// Contact clicked an activation link
            /// </summary>
            Activated = 20,

            /// <summary>
            /// Contact has opted to receive Transactional-only e-mails
            /// </summary>
            TransactionalUnsubscribed = 21,

            /// <summary>
            /// Contact's status was changed manually
            /// </summary>
            ManualStatusChange = 22,

            /// <summary>
            /// An Activation e-mail was sent
            /// </summary>
            ActivationSent = 24,

            /// <summary>
            /// Contact was deleted
            /// </summary>
            Deleted = 28,

        }

        /// <summary>
        /// History of chosen Contact
        /// </summary>
        public class ContactHistory
        {
            /// <summary>
            /// ID of history of selected Contact.
            /// </summary>
            public long ContactHistoryID { get; set; }

            /// <summary>
            /// Type of event occured on this Contact.
            /// </summary>
            public string EventType { get; set; }

            /// <summary>
            /// Numeric code of event occured on this Contact.
            /// </summary>
            public ApiTypes.ContactHistEventType EventTypeValue { get; set; }

            /// <summary>
            /// Formatted date of event.
            /// </summary>
            public string EventDate { get; set; }

            /// <summary>
            /// Name of selected channel.
            /// </summary>
            public string ChannelName { get; set; }

            /// <summary>
            /// Name of template.
            /// </summary>
            public string TemplateName { get; set; }

            /// <summary>
            /// IP Address of the event.
            /// </summary>
            public string IPAddress { get; set; }

            /// <summary>
            /// Country of the event.
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Information about the event
            /// </summary>
            public string Data { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ContactSource : Int32
        {
            /// <summary>
            /// Source of the contact is from sending an email via our SMTP or HTTP API's
            /// </summary>
            DeliveryApi = 0,

            /// <summary>
            /// Contact was manually entered from the interface.
            /// </summary>
            ManualInput = 1,

            /// <summary>
            /// Contact was uploaded via a file such as CSV.
            /// </summary>
            FileUpload = 2,

            /// <summary>
            /// Contact was added from a public web form.
            /// </summary>
            WebForm = 3,

            /// <summary>
            /// Contact was added from the contact api.
            /// </summary>
            ContactApi = 4,

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ContactStatus : Int32
        {
            /// <summary>
            /// Only transactional email can be sent to contacts with this status.
            /// </summary>
            Transactional = -2,

            /// <summary>
            /// Contact has had an open or click in the last 6 months.
            /// </summary>
            Engaged = -1,

            /// <summary>
            /// Contact is eligible to be sent to.
            /// </summary>
            Active = 0,

            /// <summary>
            /// Contact has had a hard bounce and is no longer eligible to be sent to.
            /// </summary>
            Bounced = 1,

            /// <summary>
            /// Contact has unsubscribed and is no longer eligible to be sent to.
            /// </summary>
            Unsubscribed = 2,

            /// <summary>
            /// Contact has complained and is no longer eligible to be sent to.
            /// </summary>
            Abuse = 3,

            /// <summary>
            /// Contact has not been activated or has been de-activated and is not eligible to be sent to.
            /// </summary>
            Inactive = 4,

            /// <summary>
            /// Contact has not been opening emails for a long period of time and is not eligible to be sent to.
            /// </summary>
            Stale = 5,

            /// <summary>
            /// Contact has not confirmed their double opt-in activation and is not eligible to be sent to.
            /// </summary>
            NotConfirmed = 6,

        }

        /// <summary>
        /// Number of Contacts, grouped by Status;
        /// </summary>
        public class ContactStatusCounts
        {
            /// <summary>
            /// Number of engaged contacts
            /// </summary>
            public long Engaged { get; set; }

            /// <summary>
            /// Number of active contacts
            /// </summary>
            public long Active { get; set; }

            /// <summary>
            /// Number of complaint messages
            /// </summary>
            public long Complaint { get; set; }

            /// <summary>
            /// Number of unsubscribed messages
            /// </summary>
            public long Unsubscribed { get; set; }

            /// <summary>
            /// Number of bounced messages
            /// </summary>
            public long Bounced { get; set; }

            /// <summary>
            /// Number of inactive contacts
            /// </summary>
            public long Inactive { get; set; }

            /// <summary>
            /// Number of transactional contacts
            /// </summary>
            public long Transactional { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long Stale { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long NotConfirmed { get; set; }

        }

        /// <summary>
        /// Number of Unsubscribed or Complaint Contacts, grouped by Unsubscribe Reason;
        /// </summary>
        public class ContactUnsubscribeReasonCounts
        {
            /// <summary>
            /// 
            /// </summary>
            public long Unknown { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long NoLongerWant { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long IrrelevantContent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long TooFrequent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long NeverConsented { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long DeceptiveContent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long AbuseReported { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long ThirdParty { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long ListUnsubscribe { get; set; }

        }

        /// <summary>
        /// Type of credits
        /// </summary>
        public enum CreditType : Int32
        {
            /// <summary>
            /// Used to send emails.  One credit = one email.
            /// </summary>
            Email = 9,

            /// <summary>
            /// Used to run a litmus test on a template.  1 credit = 1 test.
            /// </summary>
            Litmus = 11,

        }

        /// <summary>
        /// Daily summary of log status, based on specified date range.
        /// </summary>
        public class DailyLogStatusSummary
        {
            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public string Date { get; set; }

            /// <summary>
            /// Proper email address.
            /// </summary>
            public int Email { get; set; }

            /// <summary>
            /// Number of SMS
            /// </summary>
            public int Sms { get; set; }

            /// <summary>
            /// Number of delivered messages
            /// </summary>
            public int Delivered { get; set; }

            /// <summary>
            /// Number of opened messages
            /// </summary>
            public int Opened { get; set; }

            /// <summary>
            /// Number of clicked messages
            /// </summary>
            public int Clicked { get; set; }

            /// <summary>
            /// Number of unsubscribed messages
            /// </summary>
            public int Unsubscribed { get; set; }

            /// <summary>
            /// Number of complaint messages
            /// </summary>
            public int Complaint { get; set; }

            /// <summary>
            /// Number of bounced messages
            /// </summary>
            public int Bounced { get; set; }

            /// <summary>
            /// Number of inbound messages
            /// </summary>
            public int Inbound { get; set; }

            /// <summary>
            /// Number of manually cancelled messages
            /// </summary>
            public int ManualCancel { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Not Delivered'
            /// </summary>
            public int NotDelivered { get; set; }

        }

        /// <summary>
        /// Domain data, with information about domain records.
        /// </summary>
        public class DomainDetail
        {
            /// <summary>
            /// Name of selected domain.
            /// </summary>
            public string Domain { get; set; }

            /// <summary>
            /// True, if domain is used as default. Otherwise, false,
            /// </summary>
            public bool DefaultDomain { get; set; }

            /// <summary>
            /// True, if SPF record is verified
            /// </summary>
            public bool Spf { get; set; }

            /// <summary>
            /// True, if DKIM record is verified
            /// </summary>
            public bool Dkim { get; set; }

            /// <summary>
            /// True, if MX record is verified
            /// </summary>
            public bool MX { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool DMARC { get; set; }

            /// <summary>
            /// True, if tracking CNAME record is verified
            /// </summary>
            public bool IsRewriteDomainValid { get; set; }

            /// <summary>
            /// True, if verification is available
            /// </summary>
            public bool Verify { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public ApiTypes.TrackingType Type { get; set; }

            /// <summary>
            /// 0 - NotValidated, 1 - Validated successfully, 2 - Invalid, 3 - Broken (tracking was frequnetly verfied in given period and still is invalid). For statuses: 0, 1, 3 tracking will be verified in normal periods. For status 2 tracking will be verified in high frequent periods.
            /// </summary>
            public ApiTypes.TrackingValidationStatus TrackingStatus { get; set; }

            /// <summary>
            /// </summary>
            public ApiTypes.CertificateValidationStatus CertificateStatus { get; set; }

            /// <summary>
            /// </summary>
            public string CertificateValidationError { get; set; }

            /// <summary>
            /// </summary>
            public ApiTypes.TrackingType? TrackingTypeUserRequest { get; set; }

        }

        /// <summary>
        /// Detailed information about email credits
        /// </summary>
        public class EmailCredits
        {
            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Amount of money in transaction
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Source of URL of payment
            /// </summary>
            public string Source { get; set; }

            /// <summary>
            /// Free form field of notes
            /// </summary>
            public string Notes { get; set; }

        }

        /// <summary>
        /// </summary>
        public class EmailJobFailedStatus
        {
            /// <summary>
            /// 
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// RFC Error code
            /// </summary>
            public int ErrorCode { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Category { get; set; }

        }

        /// <summary>
        /// </summary>
        public class EmailJobStatus
        {
            /// <summary>
            /// ID number of your attachment
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// Name of status: submitted, complete, in_progress
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int RecipientsCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.EmailJobFailedStatus> Failed { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<string> Sent { get; set; }

            /// <summary>
            /// Total emails sent.
            /// </summary>
            public int SentCount { get; set; }

            /// <summary>
            /// Number of delivered messages
            /// </summary>
            public List<string> Delivered { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int DeliveredCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<string> Pending { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int PendingCount { get; set; }

            /// <summary>
            /// Number of opened messages
            /// </summary>
            public List<string> Opened { get; set; }

            /// <summary>
            /// Total emails opened.
            /// </summary>
            public int OpenedCount { get; set; }

            /// <summary>
            /// Number of clicked messages
            /// </summary>
            public List<string> Clicked { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public int ClickedCount { get; set; }

            /// <summary>
            /// Number of unsubscribed messages
            /// </summary>
            public List<string> Unsubscribed { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public int UnsubscribedCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<string> AbuseReports { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int AbuseReportsCount { get; set; }

            /// <summary>
            /// List of all MessageIDs for this job.
            /// </summary>
            public List<string> MessageIDs { get; set; }

        }

        /// <summary>
        /// </summary>
        public class EmailSend
        {
            /// <summary>
            /// ID number of transaction
            /// </summary>
            public string TransactionID { get; set; }

            /// <summary>
            /// Unique identifier for this email.
            /// </summary>
            public string MessageID { get; set; }

        }

        /// <summary>
        /// Status information of the specified email
        /// </summary>
        public class EmailStatus
        {
            /// <summary>
            /// Email address this email was sent from.
            /// </summary>
            public string From { get; set; }

            /// <summary>
            /// Email address this email was sent to.
            /// </summary>
            public string To { get; set; }

            /// <summary>
            /// Date the email was submitted.
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Value of email's status
            /// </summary>
            public ApiTypes.LogJobStatus Status { get; set; }

            /// <summary>
            /// Name of email's status
            /// </summary>
            public string StatusName { get; set; }

            /// <summary>
            /// Date of last status change.
            /// </summary>
            public DateTime StatusChangeDate { get; set; }

            /// <summary>
            /// Date when the email was sent
            /// </summary>
            public DateTime DateSent { get; set; }

            /// <summary>
            /// Date when the email changed the status to 'opened'
            /// </summary>
            public DateTime? DateOpened { get; set; }

            /// <summary>
            /// Date when the email changed the status to 'clicked'
            /// </summary>
            public DateTime? DateClicked { get; set; }

            /// <summary>
            /// Detailed error or bounced message.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// ID number of transaction
            /// </summary>
            public Guid TransactionID { get; set; }

        }

        /// <summary>
        /// Email details formatted in json
        /// </summary>
        public class EmailView
        {
            /// <summary>
            /// Body (text) of your message.
            /// </summary>
            public string Body { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Starting date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public string From { get; set; }

        }

        /// <summary>
        /// Encoding type for the email headers
        /// </summary>
        public enum EncodingType : Int32
        {
            /// <summary>
            /// Encoding of the email is provided by the sender and not altered.
            /// </summary>
            UserProvided = -1,

            /// <summary>
            /// No endcoding is set for the email.
            /// </summary>
            None = 0,

            /// <summary>
            /// Encoding of the email is in Raw7bit format.
            /// </summary>
            Raw7bit = 1,

            /// <summary>
            /// Encoding of the email is in Raw8bit format.
            /// </summary>
            Raw8bit = 2,

            /// <summary>
            /// Encoding of the email is in QuotedPrintable format.
            /// </summary>
            QuotedPrintable = 3,

            /// <summary>
            /// Encoding of the email is in Base64 format.
            /// </summary>
            Base64 = 4,

            /// <summary>
            /// Encoding of the email is in Uue format.
            /// </summary>
            Uue = 5,

        }

        /// <summary>
        /// Record of exported data from the system.
        /// </summary>
        public class Export
        {
            /// <summary>
            /// 
            /// </summary>
            public Guid PublicExportID { get; set; }

            /// <summary>
            /// Date the export was created
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// Type of export
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Current status of export
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Long description of the export
            /// </summary>
            public string Info { get; set; }

            /// <summary>
            /// Name of the file
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Link to download the export
            /// </summary>
            public string Link { get; set; }

            /// <summary>
            /// Log start date (for Type = Log only)
            /// </summary>
            public DateTime? LogFrom { get; set; }

            /// <summary>
            /// Log end date (for Type = Log only)
            /// </summary>
            public DateTime? LogTo { get; set; }

        }

        /// <summary>
        /// Type of export
        /// </summary>
        public enum ExportFileFormats : Int32
        {
            /// <summary>
            /// Export in comma separated values format.
            /// </summary>
            Csv = 1,

            /// <summary>
            /// Export in xml format
            /// </summary>
            Xml = 2,

            /// <summary>
            /// Export in json format
            /// </summary>
            Json = 3,

        }

        /// <summary>
        /// </summary>
        public class ExportLink
        {
            /// <summary>
            /// Direct URL to the exported file
            /// </summary>
            public string Link { get; set; }

        }

        /// <summary>
        /// Current status of export
        /// </summary>
        public enum ExportStatus : Int32
        {
            /// <summary>
            /// Export had an error and can not be downloaded.
            /// </summary>
            Error = -1,

            /// <summary>
            /// Export is currently loading and can not be downloaded.
            /// </summary>
            Loading = 0,

            /// <summary>
            /// Export is currently available for downloading.
            /// </summary>
            Ready = 1,

            /// <summary>
            /// Export is no longer available for downloading.
            /// </summary>
            Expired = 2,

        }

        /// <summary>
        /// Number of Exports, grouped by export type
        /// </summary>
        public class ExportTypeCounts
        {
            /// <summary>
            /// 
            /// </summary>
            public long Log { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long Contact { get; set; }

            /// <summary>
            /// Json representation of a campaign
            /// </summary>
            public long Campaign { get; set; }

            /// <summary>
            /// True, if you have enabled link tracking. Otherwise, false
            /// </summary>
            public long LinkTracking { get; set; }

            /// <summary>
            /// Json representation of a survey
            /// </summary>
            public long Survey { get; set; }

        }

        /// <summary>
        /// </summary>
        public class File
        {
            /// <summary>
            /// Name of your file.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Size of your attachment (in bytes).
            /// </summary>
            public int? Size { get; set; }

            /// <summary>
            /// Date of creation in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// When will the file be deleted from the system
            /// </summary>
            public DateTime? ExpirationDate { get; set; }

            /// <summary>
            /// Content type of the file
            /// </summary>
            public string ContentType { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum IntervalType : Int32
        {
            /// <summary>
            /// Daily overview
            /// </summary>
            Summary = 0,

            /// <summary>
            /// Hourly, detailed information
            /// </summary>
            Hourly = 1,

        }

        /// <summary>
        /// Object containig tracking data.
        /// </summary>
        public class LinkTrackingDetails
        {
            /// <summary>
            /// Number of items.
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// True, if there are more detailed data available. Otherwise, false
            /// </summary>
            public bool MoreAvailable { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.TrackedLink> TrackedLink { get; set; }

        }

        /// <summary>
        /// List of Lists, with detailed data about its contents.
        /// </summary>
        public class List
        {
            /// <summary>
            /// ID number of selected list.
            /// </summary>
            public int ListID { get; set; }

            /// <summary>
            /// Name of your list.
            /// </summary>
            public string ListName { get; set; }

            /// <summary>
            /// Number of items.
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// ID code of list
            /// </summary>
            public Guid? PublicListID { get; set; }

            /// <summary>
            /// Date of creation in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// True: Allow unsubscribing from this list. Otherwise, false
            /// </summary>
            public bool AllowUnsubscribe { get; set; }

            /// <summary>
            /// Query used for filtering.
            /// </summary>
            public string Rule { get; set; }

        }

        /// <summary>
        /// Detailed information about litmus credits
        /// </summary>
        public class LitmusCredits
        {
            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Amount of money in transaction
            /// </summary>
            public decimal Amount { get; set; }

        }

        /// <summary>
        /// Logs for selected date range
        /// </summary>
        public class Log
        {
            /// <summary>
            /// Starting date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public DateTime? From { get; set; }

            /// <summary>
            /// Ending date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public DateTime? To { get; set; }

            /// <summary>
            /// Number of recipients
            /// </summary>
            public List<ApiTypes.Recipient> Recipients { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum LogJobStatus : Int32
        {
            /// <summary>
            /// All emails
            /// </summary>
            All = 0,

            /// <summary>
            /// Email has been submitted successfully and is queued for sending.
            /// </summary>
            ReadyToSend = 1,

            /// <summary>
            /// Email has soft bounced and is scheduled to retry.
            /// </summary>
            WaitingToRetry = 2,

            /// <summary>
            /// Email is currently sending.
            /// </summary>
            Sending = 3,

            /// <summary>
            /// Email has errored or bounced for some reason.
            /// </summary>
            Error = 4,

            /// <summary>
            /// Email has been successfully delivered.
            /// </summary>
            Sent = 5,

            /// <summary>
            /// Email has been opened by the recipient.
            /// </summary>
            Opened = 6,

            /// <summary>
            /// Email has had at least one link clicked by the recipient.
            /// </summary>
            Clicked = 7,

            /// <summary>
            /// Email has been unsubscribed by the recipient.
            /// </summary>
            Unsubscribed = 8,

            /// <summary>
            /// Email has been complained about or marked as spam by the recipient.
            /// </summary>
            AbuseReport = 9,

        }

        /// <summary>
        /// Summary of log status, based on specified date range.
        /// </summary>
        public class LogStatusSummary
        {
            /// <summary>
            /// Starting date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public string From { get; set; }

            /// <summary>
            /// Ending date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public string To { get; set; }

            /// <summary>
            /// Overall duration
            /// </summary>
            public double Duration { get; set; }

            /// <summary>
            /// Number of recipients
            /// </summary>
            public long Recipients { get; set; }

            /// <summary>
            /// Number of emails
            /// </summary>
            public long EmailTotal { get; set; }

            /// <summary>
            /// Number of SMS
            /// </summary>
            public long SmsTotal { get; set; }

            /// <summary>
            /// Number of delivered messages
            /// </summary>
            public long Delivered { get; set; }

            /// <summary>
            /// Number of bounced messages
            /// </summary>
            public long Bounced { get; set; }

            /// <summary>
            /// Number of messages in progress
            /// </summary>
            public long InProgress { get; set; }

            /// <summary>
            /// Number of opened messages
            /// </summary>
            public long Opened { get; set; }

            /// <summary>
            /// Number of clicked messages
            /// </summary>
            public long Clicked { get; set; }

            /// <summary>
            /// Number of unsubscribed messages
            /// </summary>
            public long Unsubscribed { get; set; }

            /// <summary>
            /// Number of complaint messages
            /// </summary>
            public long Complaints { get; set; }

            /// <summary>
            /// Number of inbound messages
            /// </summary>
            public long Inbound { get; set; }

            /// <summary>
            /// Number of manually cancelled messages
            /// </summary>
            public long ManualCancel { get; set; }

            /// <summary>
            /// Number of messages flagged with 'Not Delivered'
            /// </summary>
            public long NotDelivered { get; set; }

            /// <summary>
            /// ID number of template used
            /// </summary>
            public bool TemplateChannel { get; set; }

        }

        /// <summary>
        /// Overall log summary information.
        /// </summary>
        public class LogSummary
        {
            /// <summary>
            /// Summary of log status, based on specified date range.
            /// </summary>
            public ApiTypes.LogStatusSummary LogStatusSummary { get; set; }

            /// <summary>
            /// Summary of bounced categories, based on specified date range.
            /// </summary>
            public ApiTypes.BouncedCategorySummary BouncedCategorySummary { get; set; }

            /// <summary>
            /// Daily summary of log status, based on specified date range.
            /// </summary>
            public List<ApiTypes.DailyLogStatusSummary> DailyLogStatusSummary { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum MessageCategory : Int32
        {
            /// <summary>
            /// 
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// 
            /// </summary>
            Ignore = 1,

            /// <summary>
            /// Number of messages marked as SPAM
            /// </summary>
            Spam = 2,

            /// <summary>
            /// Number of blacklisted messages
            /// </summary>
            BlackListed = 3,

            /// <summary>
            /// Number of messages flagged with 'No Mailbox'
            /// </summary>
            NoMailbox = 4,

            /// <summary>
            /// Number of messages flagged with 'Grey Listed'
            /// </summary>
            GreyListed = 5,

            /// <summary>
            /// Number of messages flagged with 'Throttled'
            /// </summary>
            Throttled = 6,

            /// <summary>
            /// Number of messages flagged with 'Timeout'
            /// </summary>
            Timeout = 7,

            /// <summary>
            /// Number of messages flagged with 'Connection Problem'
            /// </summary>
            ConnectionProblem = 8,

            /// <summary>
            /// Number of messages flagged with 'SPF Problem'
            /// </summary>
            SPFProblem = 9,

            /// <summary>
            /// Number of messages flagged with 'Account Problem'
            /// </summary>
            AccountProblem = 10,

            /// <summary>
            /// Number of messages flagged with 'DNS Problem'
            /// </summary>
            DNSProblem = 11,

            /// <summary>
            /// 
            /// </summary>
            NotDeliveredCancelled = 12,

            /// <summary>
            /// Number of messages flagged with 'Code Error'
            /// </summary>
            CodeError = 13,

            /// <summary>
            /// Number of manually cancelled messages
            /// </summary>
            ManualCancel = 14,

            /// <summary>
            /// Number of messages flagged with 'Connection terminated'
            /// </summary>
            ConnectionTerminated = 15,

            /// <summary>
            /// Number of messages flagged with 'Not Delivered'
            /// </summary>
            NotDelivered = 16,

        }

        /// <summary>
        /// Queue of notifications
        /// </summary>
        public class NotificationQueue
        {
            /// <summary>
            /// Creation date.
            /// </summary>
            public string DateCreated { get; set; }

            /// <summary>
            /// Date of last status change.
            /// </summary>
            public string StatusChangeDate { get; set; }

            /// <summary>
            /// Actual status.
            /// </summary>
            public string NewStatus { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Reference { get; set; }

            /// <summary>
            /// Error message.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Number of previous delivery attempts
            /// </summary>
            public string RetryCount { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum NotificationType : Int32
        {
            /// <summary>
            /// Both, email and web, notifications
            /// </summary>
            All = 0,

            /// <summary>
            /// Only email notifications
            /// </summary>
            Email = 1,

            /// <summary>
            /// Only web notifications
            /// </summary>
            Web = 2,

        }

        /// <summary>
        /// Detailed information about existing money transfers.
        /// </summary>
        public class Payment
        {
            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Amount of money in transaction
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Source of URL of payment
            /// </summary>
            public string Source { get; set; }

        }

        /// <summary>
        /// Basic information about your profile
        /// </summary>
        public class Profile
        {
            /// <summary>
            /// First name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Company name.
            /// </summary>
            public string Company { get; set; }

            /// <summary>
            /// First line of address.
            /// </summary>
            public string Address1 { get; set; }

            /// <summary>
            /// Second line of address.
            /// </summary>
            public string Address2 { get; set; }

            /// <summary>
            /// City.
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// State or province.
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// Zip/postal code.
            /// </summary>
            public string Zip { get; set; }

            /// <summary>
            /// Numeric ID of country. A file with the list of countries is available <a href="http://api.elasticemail.com/public/countries"><b>here</b></a>
            /// </summary>
            public int? CountryID { get; set; }

            /// <summary>
            /// Phone number
            /// </summary>
            public string Phone { get; set; }

            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Code used for tax purposes.
            /// </summary>
            public string TaxCode { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum QuestionType : Int32
        {
            /// <summary>
            /// 
            /// </summary>
            RadioButtons = 1,

            /// <summary>
            /// 
            /// </summary>
            DropdownMenu = 2,

            /// <summary>
            /// 
            /// </summary>
            Checkboxes = 3,

            /// <summary>
            /// 
            /// </summary>
            LongAnswer = 4,

            /// <summary>
            /// 
            /// </summary>
            Textbox = 5,

            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            Date = 6,

        }

        /// <summary>
        /// Detailed information about message recipient
        /// </summary>
        public class Recipient
        {
            /// <summary>
            /// True, if message is SMS. Otherwise, false
            /// </summary>
            public bool IsSms { get; set; }

            /// <summary>
            /// ID number of selected message.
            /// </summary>
            public string MsgID { get; set; }

            /// <summary>
            /// Ending date for search in YYYY-MM-DDThh:mm:ss format.
            /// </summary>
            public string To { get; set; }

            /// <summary>
            /// Name of recipient's status: Submitted, ReadyToSend, WaitingToRetry, Sending, Bounced, Sent, Opened, Clicked, Unsubscribed, AbuseReport
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Name of selected Channel.
            /// </summary>
            public string Channel { get; set; }

            /// <summary>
            /// Creation date
            /// </summary>
            public string Date { get; set; }

            /// <summary>
            /// Date when the email was sent
            /// </summary>
            public string DateSent { get; set; }

            /// <summary>
            /// Date when the email changed the status to 'opened'
            /// </summary>
            public string DateOpened { get; set; }

            /// <summary>
            /// Date when the email changed the status to 'clicked'
            /// </summary>
            public string DateClicked { get; set; }

            /// <summary>
            /// Content of message, HTML encoded
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// True, if message category should be shown. Otherwise, false
            /// </summary>
            public bool ShowCategory { get; set; }

            /// <summary>
            /// Name of message category
            /// </summary>
            public string MessageCategory { get; set; }

            /// <summary>
            /// ID of message category
            /// </summary>
            public ApiTypes.MessageCategory MessageCategoryID { get; set; }

            /// <summary>
            /// Date of last status change.
            /// </summary>
            public string StatusChangeDate { get; set; }

            /// <summary>
            /// Date of next try
            /// </summary>
            public string NextTryOn { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Default From: email address.
            /// </summary>
            public string FromEmail { get; set; }

            /// <summary>
            /// ID of certain mail job
            /// </summary>
            public string JobID { get; set; }

            /// <summary>
            /// True, if message is a SMS and status is not yet confirmed. Otherwise, false
            /// </summary>
            public bool SmsUpdateRequired { get; set; }

            /// <summary>
            /// Content of message
            /// </summary>
            public string TextMessage { get; set; }

            /// <summary>
            /// Comma separated ID numbers of messages.
            /// </summary>
            public string MessageSid { get; set; }

            /// <summary>
            /// Recipient's last bounce error because of which this e-mail was suppressed
            /// </summary>
            public string ContactLastError { get; set; }

        }

        /// <summary>
        /// Referral details for this account.
        /// </summary>
        public class Referral
        {
            /// <summary>
            /// Current amount of dolars you have from referring.
            /// </summary>
            public decimal CurrentReferralCredit { get; set; }

            /// <summary>
            /// Number of active referrals.
            /// </summary>
            public long CurrentReferralCount { get; set; }

        }

        /// <summary>
        /// Detailed sending reputation of your account.
        /// </summary>
        public class ReputationDetail
        {
            /// <summary>
            /// Overall reputation impact, based on the most important factors.
            /// </summary>
            public ApiTypes.ReputationImpact Impact { get; set; }

            /// <summary>
            /// Percent of Complaining users - those, who do not want to receive email from you.
            /// </summary>
            public double AbusePercent { get; set; }

            /// <summary>
            /// Percent of Unknown users - users that couldn't be found
            /// </summary>
            public double UnknownUsersPercent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double OpenedPercent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ClickedPercent { get; set; }

            /// <summary>
            /// Penalty from messages marked as spam.
            /// </summary>
            public double AverageSpamScore { get; set; }

            /// <summary>
            /// Percent of Bounced users
            /// </summary>
            public double FailedSpamPercent { get; set; }

            /// <summary>
            /// Points from quantity of your emails.
            /// </summary>
            public double RepEmailsSent { get; set; }

            /// <summary>
            /// Average reputation.
            /// </summary>
            public double AverageReputation { get; set; }

            /// <summary>
            /// Actual price level.
            /// </summary>
            public double PriceLevelReputation { get; set; }

            /// <summary>
            /// Reputation needed to change pricing.
            /// </summary>
            public double NextPriceLevelReputation { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public string PriceLevel { get; set; }

            /// <summary>
            /// True, if tracking domain is correctly configured. Otherwise, false.
            /// </summary>
            public bool TrackingDomainValid { get; set; }

            /// <summary>
            /// True, if sending domain is correctly configured. Otherwise, false.
            /// </summary>
            public bool SenderDomainValid { get; set; }

        }

        /// <summary>
        /// Reputation history of your account.
        /// </summary>
        public class ReputationHistory
        {
            /// <summary>
            /// Creation date.
            /// </summary>
            public string DateCreated { get; set; }

            /// <summary>
            /// Percent of Complaining users - those, who do not want to receive email from you.
            /// </summary>
            public double AbusePercent { get; set; }

            /// <summary>
            /// Percent of Unknown users - users that couldn't be found
            /// </summary>
            public double UnknownUsersPercent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double OpenedPercent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ClickedPercent { get; set; }

            /// <summary>
            /// Penalty from messages marked as spam.
            /// </summary>
            public double AverageSpamScore { get; set; }

            /// <summary>
            /// Points from proper setup of your account
            /// </summary>
            public double SetupScore { get; set; }

            /// <summary>
            /// Points from quantity of your emails.
            /// </summary>
            public double RepEmailsSent { get; set; }

            /// <summary>
            /// Numeric reputation
            /// </summary>
            public double Reputation { get; set; }

        }

        /// <summary>
        /// Overall reputation impact, based on the most important factors.
        /// </summary>
        public class ReputationImpact
        {
            /// <summary>
            /// Abuses - mails sent to user without their consent
            /// </summary>
            public double Abuse { get; set; }

            /// <summary>
            /// Users, that could not be reached.
            /// </summary>
            public double UnknownUsers { get; set; }

            /// <summary>
            /// Number of opened messages
            /// </summary>
            public double Opened { get; set; }

            /// <summary>
            /// Number of clicked messages
            /// </summary>
            public double Clicked { get; set; }

            /// <summary>
            /// Penalty from messages marked as spam.
            /// </summary>
            public double AverageSpamScore { get; set; }

            /// <summary>
            /// Content analysis.
            /// </summary>
            public double ServerFilter { get; set; }

            /// <summary>
            /// Tracking domain.
            /// </summary>
            public double TrackingDomain { get; set; }

            /// <summary>
            /// Sending domain.
            /// </summary>
            public double SenderDomain { get; set; }

        }

        /// <summary>
        /// Information about Contact Segment, selected by RULE.
        /// </summary>
        public class Segment
        {
            /// <summary>
            /// ID number of your segment.
            /// </summary>
            public int SegmentID { get; set; }

            /// <summary>
            /// Filename
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Query used for filtering.
            /// </summary>
            public string Rule { get; set; }

            /// <summary>
            /// Number of items from last check.
            /// </summary>
            public long LastCount { get; set; }

            /// <summary>
            /// History of segment information.
            /// </summary>
            public List<ApiTypes.SegmentHistory> History { get; set; }

        }

        /// <summary>
        /// Segment History
        /// </summary>
        public class SegmentHistory
        {
            /// <summary>
            /// ID number of history.
            /// </summary>
            public int SegmentHistoryID { get; set; }

            /// <summary>
            /// ID number of your segment.
            /// </summary>
            public int SegmentID { get; set; }

            /// <summary>
            /// Date in YYYY-MM-DD format
            /// </summary>
            public int Day { get; set; }

            /// <summary>
            /// Number of items.
            /// </summary>
            public long Count { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long EngagedCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long ActiveCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long BouncedCount { get; set; }

            /// <summary>
            /// Total emails clicked
            /// </summary>
            public long UnsubscribedCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long AbuseCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long InactiveCount { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum SendingPermission : Int32
        {
            /// <summary>
            /// Sending not allowed.
            /// </summary>
            None = 0,

            /// <summary>
            /// Allow sending via SMTP only.
            /// </summary>
            Smtp = 1,

            /// <summary>
            /// Allow sending via HTTP API only.
            /// </summary>
            HttpApi = 2,

            /// <summary>
            /// Allow sending via SMTP and HTTP API.
            /// </summary>
            SmtpAndHttpApi = 3,

            /// <summary>
            /// Allow sending via the website interface only.
            /// </summary>
            Interface = 4,

            /// <summary>
            /// Allow sending via SMTP and the website interface.
            /// </summary>
            SmtpAndInterface = 5,

            /// <summary>
            /// Allow sendnig via HTTP API and the website interface.
            /// </summary>
            HttpApiAndInterface = 6,

            /// <summary>
            /// Use access level sending permission.
            /// </summary>
            UseAccessLevel = 16,

            /// <summary>
            /// Sending allowed via SMTP, HTTP API and the website interface.
            /// </summary>
            All = 255,

        }

        /// <summary>
        /// Spam check of specified message.
        /// </summary>
        public class SpamCheck
        {
            /// <summary>
            /// Total spam score from
            /// </summary>
            public string TotalScore { get; set; }

            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public string Date { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Default From: email address.
            /// </summary>
            public string FromEmail { get; set; }

            /// <summary>
            /// ID number of selected message.
            /// </summary>
            public string MsgID { get; set; }

            /// <summary>
            /// Name of selected channel.
            /// </summary>
            public string ChannelName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.SpamRule> Rules { get; set; }

        }

        /// <summary>
        /// Single spam score
        /// </summary>
        public class SpamRule
        {
            /// <summary>
            /// Spam score
            /// </summary>
            public string Score { get; set; }

            /// <summary>
            /// Name of rule
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Description of rule.
            /// </summary>
            public string Description { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum SplitOptimization : Int32
        {
            /// <summary>
            /// Number of opened messages
            /// </summary>
            Opened = 0,

            /// <summary>
            /// Number of clicked messages
            /// </summary>
            Clicked = 1,

        }

        /// <summary>
        /// Subaccount. Contains detailed data of your Subaccount.
        /// </summary>
        public class SubAccount
        {
            /// <summary>
            /// Public key for limited access to your account such as contact/add so you can use it safely on public websites.
            /// </summary>
            public string PublicAccountID { get; set; }

            /// <summary>
            /// ApiKey that gives you access to our SMTP and HTTP API's.
            /// </summary>
            public string ApiKey { get; set; }

            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// ID number of mailer
            /// </summary>
            public string MailerID { get; set; }

            /// <summary>
            /// Name of your custom IP Pool to be used in the sending process
            /// </summary>
            public string PoolName { get; set; }

            /// <summary>
            /// Date of last activity on account
            /// </summary>
            public string LastActivity { get; set; }

            /// <summary>
            /// Amount of email credits
            /// </summary>
            public string EmailCredits { get; set; }

            /// <summary>
            /// True, if account needs credits to send emails. Otherwise, false
            /// </summary>
            public bool RequiresEmailCredits { get; set; }

            /// <summary>
            /// Amount of credits added to account automatically
            /// </summary>
            public double MonthlyRefillCredits { get; set; }

            /// <summary>
            /// True, if account needs credits to buy templates. Otherwise, false
            /// </summary>
            public bool RequiresTemplateCredits { get; set; }

            /// <summary>
            /// Amount of Litmus credits
            /// </summary>
            public decimal LitmusCredits { get; set; }

            /// <summary>
            /// True, if account is able to send template tests to Litmus. Otherwise, false
            /// </summary>
            public bool EnableLitmusTest { get; set; }

            /// <summary>
            /// True, if account needs credits to send emails. Otherwise, false
            /// </summary>
            public bool RequiresLitmusCredits { get; set; }

            /// <summary>
            /// True, if account can buy templates on its own. Otherwise, false
            /// </summary>
            public bool EnablePremiumTemplates { get; set; }

            /// <summary>
            /// True, if account can request for private IP on its own. Otherwise, false
            /// </summary>
            public bool EnablePrivateIPRequest { get; set; }

            /// <summary>
            /// Amount of emails sent from this account
            /// </summary>
            public long TotalEmailsSent { get; set; }

            /// <summary>
            /// Percent of Unknown users - users that couldn't be found
            /// </summary>
            public double UnknownUsersPercent { get; set; }

            /// <summary>
            /// Percent of Complaining users - those, who do not want to receive email from you.
            /// </summary>
            public double AbusePercent { get; set; }

            /// <summary>
            /// Percent of Bounced users
            /// </summary>
            public double FailedSpamPercent { get; set; }

            /// <summary>
            /// Numeric reputation
            /// </summary>
            public double Reputation { get; set; }

            /// <summary>
            /// Amount of emails account can send daily
            /// </summary>
            public long DailySendLimit { get; set; }

            /// <summary>
            /// Name of account's status: Deleted, Disabled, UnderReview, NoPaymentsAllowed, NeverSignedIn, Active, SystemPaused
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Maximum size of email including attachments in MB's
            /// </summary>
            public int EmailSizeLimit { get; set; }

            /// <summary>
            /// Maximum number of contacts the account can have
            /// </summary>
            public int MaxContacts { get; set; }

            /// <summary>
            /// True, if you want to use Contact Delivery Tools.  Otherwise, false
            /// </summary>
            public bool EnableContactFeatures { get; set; }

            /// <summary>
            /// Sending permission setting for account
            /// </summary>
            public ApiTypes.SendingPermission SendingPermission { get; set; }

        }

        /// <summary>
        /// Detailed account settings.
        /// </summary>
        public class SubAccountSettings
        {
            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// True, if account needs credits to send emails. Otherwise, false
            /// </summary>
            public bool RequiresEmailCredits { get; set; }

            /// <summary>
            /// True, if account needs credits to buy templates. Otherwise, false
            /// </summary>
            public bool RequiresTemplateCredits { get; set; }

            /// <summary>
            /// Amount of credits added to account automatically
            /// </summary>
            public double MonthlyRefillCredits { get; set; }

            /// <summary>
            /// Amount of Litmus credits
            /// </summary>
            public decimal LitmusCredits { get; set; }

            /// <summary>
            /// True, if account is able to send template tests to Litmus. Otherwise, false
            /// </summary>
            public bool EnableLitmusTest { get; set; }

            /// <summary>
            /// True, if account needs credits to send emails. Otherwise, false
            /// </summary>
            public bool RequiresLitmusCredits { get; set; }

            /// <summary>
            /// Maximum size of email including attachments in MB's
            /// </summary>
            public int EmailSizeLimit { get; set; }

            /// <summary>
            /// Amount of emails account can send daily
            /// </summary>
            public int DailySendLimit { get; set; }

            /// <summary>
            /// Maximum number of contacts the account can have
            /// </summary>
            public int MaxContacts { get; set; }

            /// <summary>
            /// True, if account can request for private IP on its own. Otherwise, false
            /// </summary>
            public bool EnablePrivateIPRequest { get; set; }

            /// <summary>
            /// True, if you want to use Contact Delivery Tools.  Otherwise, false
            /// </summary>
            public bool EnableContactFeatures { get; set; }

            /// <summary>
            /// Sending permission setting for account
            /// </summary>
            public ApiTypes.SendingPermission SendingPermission { get; set; }

            /// <summary>
            /// Name of your custom IP Pool to be used in the sending process
            /// </summary>
            public string PoolName { get; set; }

            /// <summary>
            /// Public key for limited access to your account such as contact/add so you can use it safely on public websites.
            /// </summary>
            public string PublicAccountID { get; set; }

        }

        /// <summary>
        /// A survey object
        /// </summary>
        public class Survey
        {
            /// <summary>
            /// Survey identifier
            /// </summary>
            public Guid PublicSurveyID { get; set; }

            /// <summary>
            /// Creation date.
            /// </summary>
            public DateTime DateCreated { get; set; }

            /// <summary>
            /// Last change date
            /// </summary>
            public DateTime? DateUpdated { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime? ExpiryDate { get; set; }

            /// <summary>
            /// Filename
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Activate, delete, or pause your survey
            /// </summary>
            public ApiTypes.SurveyStatus Status { get; set; }

            /// <summary>
            /// Number of results count
            /// </summary>
            public int ResultCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.SurveyStep> SurveySteps { get; set; }

            /// <summary>
            /// URL of the survey
            /// </summary>
            public string SurveyLink { get; set; }

        }

        /// <summary>
        /// Object with the single answer's data
        /// </summary>
        public class SurveyResultAnswerInfo
        {
            /// <summary>
            /// Answer's content
            /// </summary>
            public string content { get; set; }

            /// <summary>
            /// Identifier of the step
            /// </summary>
            public int surveystepid { get; set; }

            /// <summary>
            /// Identifier of the answer of the step
            /// </summary>
            public string surveystepanswerid { get; set; }

        }

        /// <summary>
        /// Single answer's data with user's specific info
        /// </summary>
        public class SurveyResultInfo
        {
            /// <summary>
            /// Identifier of the result
            /// </summary>
            public string SurveyResultID { get; set; }

            /// <summary>
            /// IP address
            /// </summary>
            public string CreatedFromIP { get; set; }

            /// <summary>
            /// Completion date
            /// </summary>
            public DateTime DateCompleted { get; set; }

            /// <summary>
            /// Start date
            /// </summary>
            public DateTime DateStart { get; set; }

            /// <summary>
            /// Answers for the survey
            /// </summary>
            public List<ApiTypes.SurveyResultAnswerInfo> SurveyResultAnswers { get; set; }

        }

        /// <summary>
        /// </summary>
        public class SurveyResultsAnswer
        {
            /// <summary>
            /// Identifier of the answer of the step
            /// </summary>
            public string SurveyStepAnswerID { get; set; }

            /// <summary>
            /// Number of items.
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// Answer's content
            /// </summary>
            public string Content { get; set; }

        }

        /// <summary>
        /// Data on the survey's result
        /// </summary>
        public class SurveyResultsSummaryInfo
        {
            /// <summary>
            /// Number of items.
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// Summary statistics
            /// </summary>
            public Dictionary<int, List<ApiTypes.SurveyResultsAnswer>> Summary { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum SurveyStatus : Int32
        {
            /// <summary>
            /// The survey is deleted
            /// </summary>
            Deleted = -1,

            /// <summary>
            /// The survey is not receiving result for now
            /// </summary>
            Expired = 0,

            /// <summary>
            /// The survey is active and receiving answers
            /// </summary>
            Active = 1,

        }

        /// <summary>
        /// Survey's single step info with the answers
        /// </summary>
        public class SurveyStep
        {
            /// <summary>
            /// Identifier of the step
            /// </summary>
            public int SurveyStepID { get; set; }

            /// <summary>
            /// Type of the step
            /// </summary>
            public ApiTypes.SurveyStepType SurveyStepType { get; set; }

            /// <summary>
            /// Type of the question
            /// </summary>
            public ApiTypes.QuestionType QuestionType { get; set; }

            /// <summary>
            /// Answer's content
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// Is the answer required
            /// </summary>
            public bool Required { get; set; }

            /// <summary>
            /// Sequence of the answers
            /// </summary>
            public int Sequence { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.SurveyStepAnswer> SurveyStepAnswers { get; set; }

        }

        /// <summary>
        /// Single step's answer object
        /// </summary>
        public class SurveyStepAnswer
        {
            /// <summary>
            /// Identifier of the answer of the step
            /// </summary>
            public string SurveyStepAnswerID { get; set; }

            /// <summary>
            /// Answer's content
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// Sequence of the answers
            /// </summary>
            public int Sequence { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum SurveyStepType : Int32
        {
            /// <summary>
            /// 
            /// </summary>
            PageBreak = 1,

            /// <summary>
            /// 
            /// </summary>
            Question = 2,

            /// <summary>
            /// 
            /// </summary>
            TextMedia = 3,

            /// <summary>
            /// 
            /// </summary>
            ConfirmationPage = 4,

            /// <summary>
            /// 
            /// </summary>
            ExpiredPage = 5,

        }

        /// <summary>
        /// Template
        /// </summary>
        public class Template
        {
            /// <summary>
            /// ID number of template.
            /// </summary>
            public int TemplateID { get; set; }

            /// <summary>
            /// 0 for API connections
            /// </summary>
            public ApiTypes.TemplateType TemplateType { get; set; }

            /// <summary>
            /// Filename
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Date of creation in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime DateAdded { get; set; }

            /// <summary>
            /// CSS style
            /// </summary>
            public string Css { get; set; }

            /// <summary>
            /// Default subject of email.
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Default From: email address.
            /// </summary>
            public string FromEmail { get; set; }

            /// <summary>
            /// Default From: name.
            /// </summary>
            public string FromName { get; set; }

            /// <summary>
            /// HTML code of email (needs escaping).
            /// </summary>
            public string BodyHtml { get; set; }

            /// <summary>
            /// Text body of email.
            /// </summary>
            public string BodyText { get; set; }

            /// <summary>
            /// ID number of original template.
            /// </summary>
            public int OriginalTemplateID { get; set; }

            /// <summary>
            /// Enum: 0 - private, 1 - public, 2 - mockup
            /// </summary>
            public ApiTypes.TemplateScope TemplateScope { get; set; }

        }

        /// <summary>
        /// List of templates (including drafts)
        /// </summary>
        public class TemplateList
        {
            /// <summary>
            /// List of templates
            /// </summary>
            public List<ApiTypes.Template> Templates { get; set; }

            /// <summary>
            /// List of draft templates
            /// </summary>
            public List<ApiTypes.Template> DraftTemplate { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum TemplateScope : Int32
        {
            /// <summary>
            /// Template is available for this account only.
            /// </summary>
            Private = 0,

            /// <summary>
            /// Template is available for this account and it's sub-accounts.
            /// </summary>
            Public = 1,

        }

        /// <summary>
        /// 
        /// </summary>
        public enum TemplateType : Int32
        {
            /// <summary>
            /// Template supports any valid HTML
            /// </summary>
            RawHTML = 0,

            /// <summary>
            /// Template is created and can only be modified in drag and drop editor
            /// </summary>
            DragDropEditor = 1,

        }

        /// <summary>
        /// Information about tracking link and its clicks.
        /// </summary>
        public class TrackedLink
        {
            /// <summary>
            /// URL clicked
            /// </summary>
            public string Link { get; set; }

            /// <summary>
            /// Number of clicks
            /// </summary>
            public string Clicks { get; set; }

            /// <summary>
            /// Percent of clicks
            /// </summary>
            public string Percent { get; set; }

        }

        /// <summary>
        /// </summary>
        public enum TrackingType : Int32
        {
            /// <summary>
            /// </summary>
            None = -2,

            /// <summary>
            /// </summary>
            Delete = -1,

            /// <summary>
            /// </summary>
            Http = 0,

            /// <summary>
            /// </summary>
            ExternalHttps = 1,

            /// <summary>
            /// </summary>
            InternalCertHttps = 2,

            /// <summary>
            /// </summary>
            LetsEncryptCert = 3,

        }

        /// <summary>
        /// Status of ValidDomain used by DomainValidationService to determine how often tracking validation should be performed.
        /// </summary>
        public enum TrackingValidationStatus : Int32
        {
            /// <summary>
            /// </summary>
            Validated = 0,

            /// <summary>
            /// </summary>
            NotValidated = 1,

            /// <summary>
            /// </summary>
            Invalid = 2,

            /// <summary>
            /// </summary>
            Broken = 3,

        }

        /// <summary>
        /// Account usage
        /// </summary>
        public class Usage
        {
            /// <summary>
            /// Proper email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// True, if this account is a sub-account. Otherwise, false
            /// </summary>
            public bool IsSubAccount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<ApiTypes.UsageData> List { get; set; }

        }

        /// <summary>
        /// Detailed data about daily usage
        /// </summary>
        public class UsageData
        {
            /// <summary>
            /// Date in YYYY-MM-DDThh:ii:ss format
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// Number of finished tasks
            /// </summary>
            public int JobCount { get; set; }

            /// <summary>
            /// Overall number of recipients
            /// </summary>
            public int RecipientCount { get; set; }

            /// <summary>
            /// Number of inbound emails
            /// </summary>
            public int InboundCount { get; set; }

            /// <summary>
            /// Number of attachments sent
            /// </summary>
            public int AttachmentCount { get; set; }

            /// <summary>
            /// Size of attachments sent
            /// </summary>
            public long AttachmentsSize { get; set; }

            /// <summary>
            /// Calculated cost of sending
            /// </summary>
            public decimal Cost { get; set; }

            /// <summary>
            /// Number of pricate IPs
            /// </summary>
            public int? PrivateIPCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal PrivateIPCost { get; set; }

            /// <summary>
            /// Number of SMS
            /// </summary>
            public int? SmsCount { get; set; }

            /// <summary>
            /// Overall cost of SMS
            /// </summary>
            public decimal SmsCost { get; set; }

            /// <summary>
            /// Cost of templates
            /// </summary>
            public decimal TemplateCost { get; set; }

            /// <summary>
            /// Cost of email credits
            /// </summary>
            public int? EmailCreditsCost { get; set; }

            /// <summary>
            /// Cost of template credit
            /// </summary>
            public int? TemplateCreditsCost { get; set; }

            /// <summary>
            /// Cost of litmus credits
            /// </summary>
            public decimal LitmusCost { get; set; }

            /// <summary>
            /// Cost of 1 litmus credit
            /// </summary>
            public decimal LitmusCreditsCost { get; set; }

            /// <summary>
            /// Daily cost of Contact Delivery Tools
            /// </summary>
            public decimal ContactCost { get; set; }

            /// <summary>
            /// Number of contacts
            /// </summary>
            public long ContactCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal SupportCost { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal EmailCost { get; set; }

        }


#pragma warning restore 0649
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WinHttpServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            HttpServer server = new HttpServer();
            server.Handler += server_Handler;
            server.Start(int.Parse(this.txtPort.Text));
        }

        private string server_Handler(string action, string args)
        {
            switch (action)
            {
                case "add":
                    return add(args);
                default:
                    return "ok";
            }
        }

        private string add(string args)
        {
            // a=1&b=2&c=3
            Regex regex = new Regex(@"a=(?<a>\d+)&b=(?<b>\d+)&c=(?<c>\d+)");
            int a = int.Parse(regex.Match(args).Groups["a"].Value);
            int b = int.Parse(regex.Match(args).Groups["b"].Value);
            int c = int.Parse(regex.Match(args).Groups["c"].Value);

            return (a + b + c).ToString();
        }
    }
}
/*
 POST /cloudquery.php HTTP/1.1
User-Agent: Post_Multipart
Host: 111.7.68.24
Accept: * / *
Pragma: no-cache
X-360-Cloud-Security-Desc: Scan Suspicious File
x-360-ver: 4
Content-Length: 666
Content-Type: multipart/form-data; boundary=----------------------------7166f2fc60a2

------------------------------7166f2fc60a2
Content-Disposition: form-data; name="m"


...Q............SO.R.Q
..#.wq.{D..Jy..VhQF]{...`H.t....h.....Kj;w.@..p/W.q...o...
..!c...2....VpR(vh..H...s1...G2a'xq..F.R....q-"2..._..@...P......_2..u....xT.%=nhv.ky.G.c..j..2iyRp.A.tl#..1............Os.....?....Ot...k.....Tu...1.Nt.......6.0..6..........
\...3......X.D.I.)i...j."..'..
..e.@...k..g.......>+.z........@.J,...*...`..8...`..f.....\..+.8..|2..@..`U.d.*,.Q.{h...PMT-lh..x}$&y.D....J..].!..XJ.`.....w.W..6jM.$.V.....4H@z.A.m.+..../.....o.d.....".#.@.]$..
./...Q.....T....e....I.s.,..) .f.uh.9...M..V0.....}....P.o..
------------------------------7166f2fc60a2--
HTTP/1.1 200 OK
Server: nginx
Date: Mon, 01 Feb 2021 13:42:30 GMT
Content-Type: application/octet-stream
Transfer-Encoding: chunked
Connection: close
Cache-Control: no-cache
pragma: no-cache

a1

...Q.......P..E. B
...R..0...,..ve@.....7..d.;.>?1De/.w. .[..p.=[..m....'4L.4.,....y.
2.........<.R.......................................?.?.?.?.?.?.?.?.?.?.;.
0

 * 
GET /igd.xml HTTP/1.1
Cache-Control: no-cache
Connection: Close
Pragma: no-cache
Accept: text/xml, application/xml
Host: 192.168.0.1:1900
User-Agent: Microsoft-Windows/6.1 UPnP/1.0

HTTP/1.1 200 OK
Content-Type: text/xml
Connection: close
Content-Length: 2728
Server: Wireless N Router FW300R, UPnP/1.0

<?xml version="1.0"?>
<root xmlns="urn:schemas-upnp-org:device-1-0">
<specVersion>
<major>1</major>
<minor>0</minor>
</specVersion>
 */
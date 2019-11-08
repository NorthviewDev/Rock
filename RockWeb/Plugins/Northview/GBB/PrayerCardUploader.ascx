<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCardUploader.ascx.cs" Inherits="Plugins_northview_GBB_PrayerCardUploader" %>
<link href="/Styles/GBB/filedrop.css" rel="stylesheet" />
<script src="/Scripts/GBB/filedrop.js"></script>
<script src="/Scripts/GBB/FileSaver.js"></script>
<asp:PlaceHolder ID="placeHldrPathJS" runat="server" />
<script type="text/javascript">

    var uploadArray = [];

    function uploadRequests() {

        var b64encoded = btoa(new Uint8Array(uploadArray).reduce(function (data, byte) {
                return data + String.fromCharCode(byte);
            }, ''));

        var data = {basePath: uploadPath, batch: b64encoded};

        $.ajax({
            type: "POST",
            url: "Plugins/northview/GBB/PrayerCardUploadService.asmx/UploadRequests",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                var result = JSON.parse(response.d);
                if (result.result) {
                   alert('Upload succesful!');
                }
                else {
                    alert(result.message);
                }
                
            },
            failure: function (response) {
                alert(response.d);
            }
        });

    };

    $(document).ready(function () {
        var zone = new FileDrop('zone');
        zone.multiple = true;

        zone.event('send', function (files) {
            $('.progress').removeClass('hidden');
                        
            files.each(function (file) {
                file.readData(
                    function (arr) {
                        uploadArray = arr;
                        $('#btnUpload').click();
                    },
                    function (e) { alert('An error has occurred!' + JSON.stringify(e, null, 4)) },
                    'array'
                )
            });

            $('.progress').addClass('hidden');
        });
    });

    </script>

<h2>Upload A Batch of Prayer Requests</h2>

<div class="form-group">
    <div id="zone">
        <p class="legend">
            Drop a file or click to browse
        </p>
        <p class="progress hidden">
            <span id="bar_zone" class="progress-bar progress-bar-striped active"></span>
        </p>
    </div>
    <input type="hidden" id="btnUpload" onclick="uploadRequests()" />
</div>
<div class="form-group">
    <asp:Button runat="server" ID="btnAssign" OnClick="btnAssign_Click" />
</div>
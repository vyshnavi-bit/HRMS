﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeFile="EmpdetailsImport.aspx.cs" Inherits="EmpdetailsImport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<link href="autocomplete/jquery-ui.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        $(function () {
            get_Employeedetails();
            var date = new Date();
            var day = date.getDate();
            var month = date.getMonth();
            if (month < 9) {
                month = parseFloat(month);
                month = month + 1;
                month = "0" + month;
            }
            //document.getElementById('datepicker').value = month;
            document.getElementById('selct_month').value = month;
            // $("#div_editdeduction").css("display", "block");
            //$("#div_BrandData").css("display", "block");
            $("#div_importdeduction").css("display", "block");
        });
        function callHandler(d, s, e) {
            $.ajax({
                url: 'EmployeeManagementHandler.axd',
                data: d,
                type: 'GET',
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                cache: true,
                success: s,
                error: e
            });
        }
        function CallHandlerUsingJson(d, s, e) {
            d = JSON.stringify(d);
            d = d.replace(/&/g, '\uFF06');
            d = d.replace(/#/g, '\uFF03');
            d = d.replace(/\+/g, '\uFF0B');
            d = d.replace(/\=/g, '\uFF1D');
            $.ajax({
                type: "GET",
                url: "EmployeeManagementHandler.axd?json=",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: d,
                async: true,
                cache: true,
                success: s,
                error: e
            });
        }

        function CLChange(clh) {
            if (clh.value != "") {

                var noofworkingdays = 0;
                noofworkingdays = $(clh).closest("tr").find('#txt_monthdays').val();
                var total = noofworkingdays - clh.value;
                $(clh).closest("tr").find('#txt_numbrofdays').val(total);
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <section class="content-header">
        <h1>
            Employee Total Details<small>Preview</small>
        </h1>
        <ol class="breadcrumb">
            <li><a href="#"><i class="fa fa-dashboard"></i>Details Management</a></li>
            <li><a href="#">Empdetails</a></li>
        </ol>
    </section>
    <section class="content">
        <div class="box box-info">
        
             
               <div id="div_importdeduction">
        <asp:UpdateProgress ID="updateProgress1" runat="server">
        <ProgressTemplate>
            <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0;
                right: 0; left: 0; z-index: 9999999; background-color: #FFFFFF; opacity: 0.7;">
                <br />
                <asp:Image ID="imgUpdateProgress" runat="server" ImageUrl="thumbnails/loading.gif"
                    AlternateText="Loading ..." ToolTip="Loading ..." Style="padding: 10px; position: absolute;
                    top: 35%; left: 40%;" />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <%-- <asp:UpdatePanel ID="updPanel" runat="server">
                        <ContentTemplate>--%>
                        
                <div class="box-header with-border">
                    <h3 class="box-title">
                        <i style="padding-right: 5px;" class="fa fa-cog"></i>Employee Import Details
                    </h3>
                </div>
                <div class="box-body">
                    <table>
                        <tr>
                         <td>
                             <asp:UpdatePanel ID="updPanel132" runat="server">
                       <%-- <ContentTemplate>
                                            <asp:Label ID="Label3" runat="server" Text="Label">EmployeeType</asp:Label>&nbsp;
                                            <asp:DropDownList ID="ddlemployee" runat="server" AutoPostBack="True" onselectedindexchanged="ddlemployee_SelectedIndexChanged">
                                          </asp:DropDownList>

                                         </ContentTemplate>--%>
                </asp:UpdatePanel>   
                                        </td>
                            <td>
                                <asp:FileUpload ID="FileUploadToServer" runat="server" Style="height: 25px; font-size: 16px;" />
                            </td>
                            <td style="width: 5px;">
                            </td>
                            <td>
                                <asp:Button ID="btnImport" runat="server" Text="Import" class="btn btn-primary"
                                    OnClick="btnImport_Click" />
                            </td>
                            <td>
                            <asp:Label ID="Label1" runat="server" Text=" NOTE : Date formet must be MM/DD/YYYY"  Font-Bold="True" ForeColor="Red"></asp:Label>
                            </td>
                        </tr>
                    </table>
                    <br />
                    <asp:UpdatePanel ID="updPanel" runat="server">
                        <ContentTemplate>
                   
                            <ContentTemplate>
                            <asp:GridView ID="grvExcelData" runat="server" ForeColor="White" Width="100%" CssClass="gridcls"
                                GridLines="Both" Font-Bold="true" Font-Size="Smaller">
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="Gray" Font-Bold="False" ForeColor="White" />
                                <HeaderStyle BackColor="#f4f4f4" Font-Bold="False" ForeColor="Black" Font-Italic="False"
                                    Font-Names="Raavi" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#ffffff" ForeColor="#333333" HorizontalAlign="Center" />
                                <AlternatingRowStyle HorizontalAlign="Center" />
                                <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" />
                            </asp:GridView>
                            </dr>
                           <%-- <asp:Button ID="btnsave" runat="server" Text="Save" class="btn btn-primary" OnClick="btnSave_Click" />
                              <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/exporttoxl.aspx">Export to XL</asp:HyperLink>--%>
                        </ContentTemplate>
                            </dr>
                            <asp:Button ID="btnsave" runat="server" Text="Save" class="btn btn-primary"
                                OnClick="btnSave_Click" />
                                <br />
                                <br />
                    <asp:Label ID="lblmsg" runat="server"  Font-Bold="True" ForeColor="Red"></asp:Label><br />

                                <br />
                                <br />
                                <br />
                                <br />


                                            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/exporttoxl.aspx">Export to XL</asp:HyperLink>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                 <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                <div>
                 <asp:Label ID="lblMessage" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label><br />
                </div>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>
              
       
       </div>
        
    </section>
</asp:Content>


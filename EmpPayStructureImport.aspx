﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeFile="EmpPayStructureImport.aspx.cs" Inherits="EmpPayStructureImport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
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
    <div>
        <section class="content-header">
            <h1>
                Employee Paystructure Import Data<small>Preview</small>
            </h1>
            <ol class="breadcrumb">
                <li><a href="#"><i class="fa fa-dashboard"></i>Reports</a></li>
                <li><a href="#">Employee Paystructure Import Data</a></li>
            </ol>
            <div class="box box-info">
                <div class="box-header with-border">
                    <h3 class="box-title">
                        <i style="padding-right: 5px;" class="fa fa-cog"></i>Employee Paystructure Import Details
                    </h3>
                </div>
                <div class="box-body">
                    <table>
                        <tr>
                            <td>
                                <asp:FileUpload ID="FileUploadToServer" runat="server" Style="height: 25px; font-size: 16px;" />
                            </td>
                            <td style="width: 5px;">
                            </td>
                            <td>
                                <asp:Button ID="btnImport" runat="server" Text="Import" class="btn btn-primary" OnClick="btnImport_Click" />
                            </td>
                        </tr>
                    </table>
                    <br />
                    <asp:UpdatePanel ID="updPanel" runat="server">
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
                            <asp:Button ID="btnsave" runat="server" Text="Save" class="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:Label ID="lblMessage" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label><br />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                 <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                <div>
                 <asp:Label ID="lblmsg" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label><br />
                </div>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </section>
    </div>
</asp:Content>


<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="WebApp.index" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <table>
            <tr>
                <td colspan="2">
                    <asp:Label ID="Label5" runat="server" Text="Käyttäjä:"></asp:Label>
                    <asp:TextBox ID="TextBoxCurrentUser" runat="server" Width="220px"></asp:TextBox>
                    <asp:Button ID="ButtonGetCurrentUser" runat="server" Text="Hae käyttäjä" OnClick="ButtonGetCurrenUser_ClickAsync" />
                    <asp:Button ID="ButtonRefresh" runat="server" Text="Päivitä" onClick="ButtonRefresh_ClickAsync"/>
                    <button onclick='javascript:window.open("https://github.com/Kettja12/WebApp");return false'>Lähdekoodit</button>
                    <button onclick='javascript:window.open("https://jarikettunen.ddns.net/data/SharedSessiondataTest.side");return false'>Selenium IDE testit</button>
                    <button onclick='javascript:window.open("https://www.selenium.dev/selenium-ide/");return false'>Selenium IDE laajennus selaimeen</button>
                    
                </td>
            </tr>
            <tr>
                <td style="height: 18px">
                    <asp:Label ID="LabelCurrentUser" runat="server"
                        style="margin-left:60px"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <table>
                        <tr>
                            <td>
                                <asp:ListBox ID="ListBoxUsers" runat="server" Width="138px" Height="400px"
                                    OnSelectedIndexChanged="ListBoxUsers_SelectedIndexChanged"
                                    AutoPostBack="true"></asp:ListBox>
                            </td>
                        </tr>
                    </table>
                </td>
                <td>
                    <table>
                        <tr>
                            <td>
                                <asp:Label ID="Label4" runat="server" Text="ID: "></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="TextBoxId" runat="server" Width="220px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label1" runat="server" Text="Käyttäjätunnus: "></asp:Label>

                            </td>
                            <td>
                                <asp:TextBox ID="TextBoxUsername" runat="server" Width="220px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label2" runat="server" Text="Etunimi: "></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="TextBoxFirstName" runat="server" Width="220px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label3" runat="server" Text="Sukunimi: "></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="TextBoxLastname" runat="server" Width="220px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Claims</td>
                            <td>
                                <asp:ListBox ID="ListBoxClaims" runat="server" Width="300px"
                                    AutoPostBack="true"></asp:ListBox>
                                <asp:TextBox ID="TextBoxClaim" runat="server" Width="220px"></asp:TextBox>
                                <asp:Button ID="ButtonAddClaim" runat="server" Text="lisää claim" OnClick="ButtonAddClaim_ClickAsync" />
                            </td>
                        </tr>

                        <tr>
                            <td></td>
                            <td>
                                <asp:Button ID="ButtonSave" runat="server" Text="Tallenna" OnClick="ButtonSave_ClickAsync" />
                                <asp:Button ID="ButtonUndo" runat="server" Text="Peru viimeisin (0) " OnClick="ButtonUndo_ClickAsync" />
                                <asp:Button ID="ButtonUndoAll" runat="server" Text="Peru kaikki" OnClick="ButtonUndoAll_ClickAsync" />
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <asp:Button ID="ButtonClear" runat="server" Text="Poista valinta näytöltä" OnClick="ButtonClear_Click" />
                                <asp:Button ID="ButtonDelete" runat="server" Text="Poista" OnClick="ButtonDelete_ClickAsync" />
                                <asp:Button ID="ButtonAdd" runat="server" Text="Lisää" OnClick="ButtonAdd_ClickAsync" />
                                <asp:Button ID="ButtonSaveToDb" runat="server" Text="Tallenna tietokantaan" OnClick="ButtonSaveToDb_ClickAsync" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="height: 18px;">
                                <asp:Label ID="LabelModified" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="height: 18px;">
                                <asp:Label ID="LabelErrorMessage" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>

    </form>
</body>
</html>

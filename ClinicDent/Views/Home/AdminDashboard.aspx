<%@ Page Title="Panel Administrativo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="ClinicaDental.Admin.AdminDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h2>Panel Administrativo</h2>
        
        <div class="row">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        Gestión de Usuarios
                    </div>
                    <div class="card-body">
                        <p>Administra los usuarios del sistema y sus roles.</p>
                        <asp:Button ID="btnGestionUsuarios" runat="server" Text="Gestionar Usuarios" 
                            CssClass="btn btn-primary" OnClick="btnGestionUsuarios_Click" />
                    </div>
                </div>
            </div>
            
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header bg-success text-white">
                        Gestión de Tipos de Cobro
                    </div>
                    <div class="card-body">
                        <p>Administra los tipos de cobro disponibles para los tratamientos.</p>
                        <asp:Button ID="btnGestionTiposCobro" runat="server" Text="Gestionar Tipos de Cobro" 
                            CssClass="btn btn-success" OnClick="btnGestionTiposCobro_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
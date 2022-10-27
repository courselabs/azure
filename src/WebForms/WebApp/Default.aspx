<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Azure Labs</h1>
        <p class="lead">Learn to use Azure with self-paced labs!</p>
        <p><a href="https://azure.courselabs.co" class="btn btn-primary btn-lg">Azure course labs &raquo;</a></p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>IaaS</h2>
            <p>
                Infrastructure-as-a-Service
            </p>
        </div>
        <div class="col-md-4">
            <h2>PaaS</h2>
            <p>
                Platform-as-a-Service
            </p>
        </div>
        <div class="col-md-4">
            <h2>CaaS</h2>
            <p>
                Containers-as-a-Service
            </p>
        </div>
    </div>

</asp:Content>

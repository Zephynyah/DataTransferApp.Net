
```xml
<Window x:Class="DataTransferApp.Net.Views.ChangesWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:fa="http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp"
        xmlns:markdown="clr-namespace:WPFUI.Markdown;assembly=WPFUI.Markdown"
        mc:Ignorable="d"
        Title="What's New" Height="700" Width="800"
        WindowStartupLocation="CenterScreen"
        ResizeMode="CanResize"
        Background="{StaticResource BackgroundBrush}">

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" HorizontalAlignment="Center" Margin="0,0,0,20">
            <Image Source="/app.png" Width="48" Height="48" Margin="0,0,15,0"/>
            <StackPanel VerticalAlignment="Center">
                <TextBlock Text="What's New" FontSize="24" FontWeight="Bold" Foreground="#2C3E50"/>
                <TextBlock Text="Version History &amp; Updates" FontSize="14" Foreground="#7F8C8D"/>
            </StackPanel>
        </StackPanel>

        <!-- Changelog Content -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto">
            <TextBox x:Name="ChangelogTextBox"
                     Text="{Binding ChangelogContent, Mode=OneWay}"
                     IsReadOnly="True"
                     Background="Transparent"
                     BorderThickness="0"
                     FontFamily="Consolas"
                     FontSize="12"
                     TextWrapping="Wrap"
                     AcceptsReturn="True"
                     VerticalScrollBarVisibility="False"
                     HorizontalScrollBarVisibility="Auto"
                     Padding="10"
                     Foreground="#34495E"/>
        </ScrollViewer>

        <!-- Footer -->
        <StackPanel Grid.Row="2" HorizontalAlignment="Center" Margin="0,20,0,0">
            <Button Content="Close"
                    HorizontalAlignment="Center"
                    Padding="20,8"
                    Background="#3498DB"
                    Foreground="{StaticResource BackgroundBrush}"
                    BorderThickness="0"
                    FontSize="14"
                    Click="CloseButton_Click"
                    Cursor="Hand"/>
        </StackPanel>
    </Grid>
</Window>
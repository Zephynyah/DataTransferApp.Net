```xml

<DataGrid.RowStyle>
    <Style TargetType="DataGridRow">
        <Setter Property="ToolTip">
            <Setter.Value>
                <ToolTip MaxWidth="400" Visibility="{Binding HasError, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <StackPanel>
                        <!-- Header with icon -->
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,5">
                            <fa:IconBlock Icon="File" FontSize="16" Margin="0,0,8,0" Foreground="{StaticResource PrimaryBrush}"/>
                            <TextBlock Text="{Binding FileName}" FontWeight="Bold" FontSize="14"/>
                        </StackPanel>
                        
                        <Separator Margin="0,0,0,5"/>
                        
                        <!-- File info -->
                        <TextBlock Text="{Binding FullPath}" FontSize="11" Foreground="Gray" Margin="0,0,0,3"/>
                        <TextBlock Text="{Binding SizeFormatted}" FontSize="11" Foreground="Gray" Margin="0,0,0,8"/>
                        
                        <!-- Blacklist warning -->
                        <Border Background="#FFF3CD" Padding="8" CornerRadius="3" Margin="0,0,0,5"
                                Visibility="{Binding IsBlacklisted, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <StackPanel>
                                <StackPanel Orientation="Horizontal" Margin="0,0,0,3">
                                    <fa:IconBlock Icon="Ban" Foreground="#856404" FontSize="14" Margin="0,0,5,0"/>
                                    <TextBlock Text="Blacklisted File Type" FontWeight="Bold" Foreground="#856404"/>
                                </StackPanel>
                                <TextBlock Text="💡 Remove or change extension before transfer" 
                                           FontSize="11" Foreground="#856404" TextWrapping="Wrap"/>
                            </StackPanel>
                        </Border>
                        
                        <!-- Compressed file warning -->
                        <Border Background="#FFF3CD" Padding="8" CornerRadius="3"
                                Visibility="{Binding IsCompressed, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <StackPanel>
                                <StackPanel Orientation="Horizontal" Margin="0,0,0,3">
                                    <fa:IconBlock Icon="Archive" Foreground="#856404" FontSize="14" Margin="0,0,5,0"/>
                                    <TextBlock Text="Compressed File" FontWeight="Bold" Foreground="#856404"/>
                                </StackPanel>
                                <TextBlock Text="💡 Extract contents before transfer" 
                                           FontSize="11" Foreground="#856404" TextWrapping="Wrap"/>
                            </StackPanel>
                        </Border>
                    </StackPanel>
                </ToolTip>
            </Setter.Value>
        </Setter>
    </Style>
</DataGrid.RowStyle>
```
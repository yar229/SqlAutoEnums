

$dataSource = “wad.audiomania.ru,1443”
$user = "developer"
$pwd = "Jobsisdead"
$database = "Warehouse"
$query = "select * from AM_Enumerations"
$queryL = "select distinct EnumPrefix, Category from AM_Enumerations"   
$path = "C:\\Users\yar\\AppData\\Local\\Red Gate\\SQL Prompt 5\\Snippets"
$content = 
"<?xml version=""1.0"" encoding=""utf-16""?>
<CodeSnippets>
  <CodeSnippet Format=""1.0.0"">
    <Header>
      <Title>{0}</Title>
      <Shortcut>{1}</Shortcut>
      <Description>{2}</Description>
      <Author />
      <SnippetTypes>
        <SnippetType>Expansion</SnippetType>
      </SnippetTypes>
    </Header>
    <Snippet>
      <Declarations />
      <Code Language=""sql""><![CDATA[{3}]]></Code>
    </Snippet>
  </CodeSnippet>
</CodeSnippets>
"

$connectionString = “Server=$dataSource;uid=$user; pwd=$pwd;Database=$database;Integrated Security=False;”

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString

$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = $query

$result = $command.ExecuteReader()

$table = new-object “System.Data.DataTable”
$table.Load($result)


#1..5 | % { New-Item  -Name "$_.txt" -Value (Get-Date).toString() -ItemType file -Force}
foreach ($Row in $table.Rows)
{ 
  #write-host "value is : $($Row[""Category""])"
  
  $filename = "$($Row[""EnumPrefix""])$($Row[""Category""])$($Row[""EnumMemberName""]).sqlpromptsnippet"
  $title = "$($Row[""EnumPrefix""])$($Row[""Category""])::$($Row[""EnumMemberName""])"
  $shortcut = "$($Row[""EnumPrefix""])$($Row[""Category""])::$($Row[""EnumMemberName""])"
  $description = "[$($Row[""Code""])] $($Row[""Name""])"
  $data = "$($Row[""EnumPrefix""])$($Row[""Category""])::$($Row[""EnumMemberName""])"

  $value = [string]::Format($content, $title, $shortcut, $description, $data)

  #New-Item -Name $filename -Path $path -Value $value  -ItemType File -Force -Encoding ascii

  out-file -InputObject $value -FilePath "$path\\$filename" -Force -Encoding Unicode

}

#=========================================================================================

$commandL = $connection.CreateCommand()
$commandL.CommandText = $queryL

$resultL = $commandL.ExecuteReader()

$tableL = new-object “System.Data.DataTable”
$tableL.Load($resultL)

foreach ($RowL in $tableL.Rows)
{ 
  $filenameL = "$($RowL[""EnumPrefix""])$($RowL[""Category""])ToList.sqlpromptsnippet"
  $titleL = "$($RowL[""EnumPrefix""])$($RowL[""Category""]).ToList"
  $shortcutL = "$($RowL[""EnumPrefix""])$($RowL[""Category""]).ToList"
  $descriptionL = "-----members list---------------------------------------"
  $dataL = "[$($RowL[""EnumPrefix""])$($RowL[""Category""]).ToList]()"

  $valueL = [string]::Format($content, $titleL, $shortcutL, $descriptionL, $dataL)

  #New-Item -Name $filenameL -Path $path -Value $valueL  -ItemType File -Force -Encoding ascii

  out-file -InputObject $valueL -FilePath "$path\\$filenameL" -Force -Encoding Unicode

}



$connection.Close()
# http://stackoverflow.com/a/39648656

$gitUser       = git config user.name;
$gitEmail      = git config user.email;
$gitStateUser  = git --no-pager log -1 --pretty=format:"%an";
$gitStateEmail = git --no-pager log -1 --pretty=format:"%ae";
$gitHashShort  = git rev-parse --short HEAD;
$gitHashLong   = git rev-parse HEAD;
$gitLastTag    = git describe --tags --abbrev=0;
$gitOwnerRepo  = [regex]::Match((git config --get remote.origin.url), "(?:github.com\/)(.+?)\/([^.\n]*)");
$gitOwner      = $gitOwnerRepo.Groups[1].Value;
$gitRepo       = $gitOwnerRepo.Groups[2].Value;
$gitBranch     = git name-rev --name-only HEAD;
$gitCount      = git rev-list HEAD --count;
$compileTime   = [DateTime]::UtcNow.ToString('u').Replace('Z','UTC');

$projectPath = $args[0].Replace("""","");
$assemblyFile = $projectPath + "\Properties\AssemblyInfo.cs";
$templateAssemblyFile = $projectPath + "\Properties\AssemblyInfo_template.cs";
$versionFile = $projectPath + "\Version.cs";
$templateVersionFile = $projectPath + "\Version_template.cs";

# Read template files, overwrite place holders with git version info
$newAssemblyContent = Get-Content $templateAssemblyFile |
    %{$_ -replace '\$gitUser\$', $gitUser.Replace('"','\"') } |
    %{$_ -replace '\$gitEmail\$', $gitEmail } |
    %{$_ -replace '\$gitStateUser\$', $gitStateUser.Replace('"','\"') } |
    %{$_ -replace '\$gitStateEmail\$', $gitStateEmail } |
    %{$_ -replace '\$gitHashShort\$', $gitHashShort } |
    %{$_ -replace '\$gitHashLong\$', $gitHashLong } |
    %{$_ -replace '\$gitLastTag\$', $gitLastTag } |
    %{$_ -replace '\$gitOwner\$', $gitOwner } |
    %{$_ -replace '\$gitRepo\$', $gitRepo } |
    %{$_ -replace '\$gitBranch\$', $gitBranch } |
    %{$_ -replace '\$gitCount\$', $gitCount } |
    %{$_ -replace '\$compileTime\$', $compileTime };

$newVersionContent = Get-Content $templateVersionFile |
    %{$_ -replace '\$gitUser\$', $gitUser.Replace('"','\"') } |
    %{$_ -replace '\$gitEmail\$', $gitEmail } |
    %{$_ -replace '\$gitStateUser\$', $gitStateUser.Replace('"','\"') } |
    %{$_ -replace '\$gitStateEmail\$', $gitStateEmail } |
    %{$_ -replace '\$gitHashShort\$', $gitHashShort } |
    %{$_ -replace '\$gitHashLong\$', $gitHashLong } |
    %{$_ -replace '\$gitLastTag\$', $gitLastTag } |
    %{$_ -replace '\$gitOwner\$', $gitOwner } |
    %{$_ -replace '\$gitRepo\$', $gitRepo } |
    %{$_ -replace '\$gitBranch\$', $gitBranch } |
    %{$_ -replace '\$gitCount\$', $gitCount } |
    %{$_ -replace '\$compileTime\$', $compileTime };

# Write cs files only if there are changes
Write-Host " * Updated Version.cs";
$newVersionContent > $versionFile;

Write-Host " * Updated AssemblyInfo.cs";
$newAssemblyContent > $assemblyFile;
$projectName = "configurationserver"
$oldCert = get-childItem -path cert:\CurrentUser\My | Where-Object {$_.FriendlyName -match "ASP.NET Core HTTPS development certificate"}
New-SelfSignedCertificate `
	-CloneCert $oldCert `
	-Subject $projectName `
	-DnsName $projectName `
	-FriendlyName "gRPC HTTPS dev certificate" `
	-KeyUsage CertSign, DigitalSignature, KeyEncipherment `
	-CertStoreLocation "Cert:\CurrentUser\My"
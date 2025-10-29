
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
# Base URL
$baseUrl = "http://localhost:5163"

# Test user info
$username = "apitestuser"
$password = "Password123!"
$email = "$username@example.com"

# Register
Write-Host "`n📝 Registering user..."
$registerBody = @{
    username = $username
    email = $email
    password = $password
    firstName = "API"
    lastName = "Tester"
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method POST -Body $registerBody -ContentType "application/json"
    Write-Host "✅ Registered successfully"
} catch {
    Write-Host "⚠️  Registration failed (possibly already exists): $($_.Exception.Message)"
}

# Login
Write-Host "`n🔐 Logging in..."
$loginBody = @{
    username = $username
    password = $password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✅ Logged in. Token acquired."
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)"
    exit
}


Write-Host $token

# Auth header
$headers = @{ Authorization = "Bearer $token" }

# Get all users
Write-Host "`n👥 Getting all users..."
try {
    $users = Invoke-RestMethod -Uri "$baseUrl/users" -Method GET -Headers $headers
    Write-Host "✅ Retrieved $($users.Count) users."
} catch {
    Write-Host "❌ Failed to get users: $($_.Exception.Message)"
}

# Pick one user to test further
$userId = $users[0].id

# Get user by ID
Write-Host "`n🔍 Getting user by ID: $userId"
try {
    $user = Invoke-RestMethod -Uri "$baseUrl/users/$userId" -Method GET -Headers $headers
    Write-Host "✅ User found: $($user.username)"
} catch {
    Write-Host "❌ Failed to get user: $($_.Exception.Message)"
}

# Update user
Write-Host "`n✏️ Updating user..."
$updateBody = @{
    email = "updated_$email"
    firstName = "Updated"
    lastName = "User"
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$baseUrl/users/$userId" -Method PUT -Headers $headers -Body $updateBody -ContentType "application/json"
    Write-Host "✅ User updated."
} catch {
    Write-Host "❌ Failed to update user: $($_.Exception.Message)"
}

# Delete user
Write-Host "`n🗑️ Deleting user..."
try {
    Invoke-RestMethod -Uri "$baseUrl/users/$userId" -Method DELETE -Headers $headers
    Write-Host "✅ User deleted."
} catch {
    Write-Host "❌ Failed to delete user: $($_.Exception.Message)"
}

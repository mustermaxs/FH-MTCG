

// IRGENDWO & IRGENDWIE
// aus der Main das controller und route über attributes
// im EndpointMapper registrieren


/// AD Controllermethodenargumente
/// falls es möglich ist methode ohne "Invoke" aufzurufen
/// könnte man Argumente aus dem dictionary vlt "benannt"
/// an methode übergeben, also
/// theMethod({username = req.TryGetValue("username")})
/// ANSONSTEN
/// wsl am leichtesten einfach request object
/// über controller constructor übergeben...
# FitwomanAPI - Backend REST API (.NET 8)

Backend API centralizada para el ecosistema **Fitwoman**, construida en **.NET 8 Web API** con **PostgreSQL** y arquitectura de autenticación segura JWT. Esta API está diseñada para alimentar dos portales web independientes (Portal Administrativo y Portal Cliente).

---

## 🛠️ Prerrequisitos de Software

Antes de ejecutar este proyecto, asegúrate de contar con los siguientes elementos instalados en tu sistema:

* **.NET 8.0 SDK** (o posterior): [Descargar .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
* **PostgreSQL** (v14 o superior): Servidor de base de datos relacional activo.
* **Herramientas de Entity Framework Core CLI** (opcional pero recomendado para migraciones):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## ⚙️ Configuración y Variables de Entorno

El proyecto lee la configuración desde `appsettings.json` (o `appsettings.Development.json` / `User Secrets` en desarrollo local).

Asegúrate de tener configuradas las siguientes claves en tu `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fitwoman_db;Username=postgres;Password=TU_CONTRASEÑA_POSTGRES"
  },
  "Jwt": {
    "Issuer": "FitwomanAPI",
    "Audience": "FitwomanApps",
    "SecretKey": "Tu_Clave_Secreta_Super_Segura_De_Minimo_32_Caracteres!",
    "AccessTokenExpirationMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ **Nota de Seguridad:** En entornos de producción, la clave `Jwt:SecretKey` y la cadena de conexión `DefaultConnection` no deben comitearse en el control de versiones. Utiliza variables de entorno o gestores de secretos (como Azure Key Vault o User Secrets).

---

## 🚀 Cómo Levantar el Proyecto

Sigue estos pasos para ejecutar la API localmente:

### 1. Clonar el repositorio e ingresar a la carpeta
```bash
git clone https://github.com/saraiherrerab/fitness-business-backend.git
cd FitwomanAPI
```

### 2. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 3. Aplicar migraciones a la Base de Datos PostgreSQL
Asegúrate de que PostgreSQL esté corriendo y ejecuta:
```bash
dotnet ef database update
```

### 4. Compilar y ejecutar la aplicación
```bash
dotnet run
```

Una vez iniciada la aplicación, la API estará escuchando (por defecto) en `https://localhost:7001` o `http://localhost:5000`.

---

## 📑 Documentación e Interfaz Interactiva (Swagger)

En entorno de desarrollo (`Development`), la documentación interactiva OpenAPI/Swagger está habilitada por defecto:

👉 **URL de Swagger UI:** `https://localhost:<puerto>/swagger`

* **Probar Endpoints Protegidos:** Haz clic en el botón verde **Authorize** arriba a la derecha e ingresa tu token en el formato:
  `Bearer <TU_TOKEN_JWT>`

---

## 📦 Dependencias Clave (Paquetes NuGet)

| Paquete NuGet | Versión | Propósito / Uso |
| --- | --- | --- |
| **`Npgsql.EntityFrameworkCore.PostgreSQL`** | `8.0.10` | Proveedor ORM EF Core oficial para PostgreSQL. |
| **`Microsoft.EntityFrameworkCore.Design`** | `8.0.10` | Herramientas de diseño para generación de migraciones y scaffolding. |
| **`Microsoft.AspNetCore.Authentication.JwtBearer`** | `8.0.10` | Middleware para validación de tokens JWT en endpoints protegidos. |
| **`BCrypt.Net-Next`** | `4.2.0` | Algoritmo de hashing adaptativo para cifrado seguro de contraseñas de usuarios. |
| **`Swashbuckle.AspNetCore`** | `6.6.2` | Generación automática de especificación Swagger/OpenAPI e interfaz web interactiva. |

---

## 🔐 Seguridad y Autenticación

- **Hashing de Contraseñas:** Se utiliza BCrypt con sal automática para prevenir ataques de diccionario y rainbow tables.
- **Cookies `HttpOnly` + `SameSite`:** El endpoint de inicio de sesión (`POST /api/auth/login`) emite automáticamente una cookie `HttpOnly` con el token JWT para protección contra ataques XSS.
- **Soporte CORS:** Habilitado en `Program.cs` para permitir llamadas seguras desde los portales de desarrollo frontend (`localhost:5173`, `localhost:3000`, etc.) incluyendo soporte para credenciales/cookies.

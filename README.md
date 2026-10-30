# FirstOrdersApi (.NET 10 Web API)

Backend for the Angular menu app. JWT login, role-based endpoints, in-memory data, CORS for `http://localhost:4200`.

## Run

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```powershell
dotnet run
```

API runs at http://localhost:5000. Try `FirstOrdersApi.http` (VS Code REST Client / Visual Studio) to call each endpoint.

## Endpoints

| Method | URL                        | Who                | Notes                           |
|--------|----------------------------|--------------------|---------------------------------|
| POST   | /api/auth/login            | anyone             | `{username, password}` returns `{token, user}` |
| GET    | /api/auth/me               | signed in          |                                 |
| GET    | /api/products              | signed in          | `?search=` `&category=`         |
| GET    | /api/products/{id}         | signed in          |                                 |
| POST   | /api/products              | manager, admin     | 201 + Location header           |
| PUT    | /api/products/{id}         | manager, admin     |                                 |
| DELETE | /api/products/{id}         | admin              | 204                             |
| GET    | /api/orders, /{id}         | manager, admin     | `?status=`                      |
| POST   | /api/orders                | manager, admin     |                                 |
| PUT    | /api/orders/{id}           | manager, admin     |                                 |
| DELETE | /api/orders/{id}           | admin              |                                 |
| GET    | /api/users, /{id}          | admin              |                                 |
| PUT    | /api/users/{id}/role       | admin              | `{ "role": "manager" }`         |
| DELETE | /api/users/{id}            | admin              | cannot delete yourself          |

Errors: 400 (validation, `{message}`), 401 (no/invalid token), 403 (wrong role), 404.

Demo logins: `admin/admin123`, `manager/manager123`, `customer/customer123`.

## Connecting the Angular app

### 1. Use a dev proxy (recommended)

The Angular interceptor only adds the bearer token to **relative** URLs, so call `/api/...` and let the dev server proxy it.

Create `proxy.conf.json` next to `package.json`:

```json
{
  "/api": { "target": "http://localhost:5000", "secure": false }
}
```

In `angular.json`, change the `serve` target:

```json
"serve": {
  "builder": "@angular-devkit/build-angular:dev-server",
  "options": { "proxyConfig": "proxy.conf.json" },
  ...
}
```

Restart `npm start`.

### 2. Point `DummyDataService` at the API

```ts
getProducts()  { return this.http.get<Product[]>('/api/products'); }
getOrders()    { return this.http.get<Order[]>('/api/orders'); }
getUsers()     { return this.http.get<User[]>('/api/users'); }
updateUserRole(id: number, role: Role) {
  return this.http.put<User>(`/api/users/${id}/role`, { role });
}
createProduct(p: Omit<Product, 'id'>)       { return this.http.post<Product>('/api/products', p); }
updateProduct(id: number, p: Omit<Product, 'id'>) { return this.http.put<Product>(`/api/products/${id}`, p); }
deleteProduct(id: number)                   { return this.http.delete<void>(`/api/products/${id}`); }
```

### 3. Use the API for sign-in in `AuthService`

```ts
private http = inject(HttpClient);

login(username: string, password: string): Observable<boolean> {
  return this.http.post<{ token: string; user: User }>('/api/auth/login', { username, password }).pipe(
    tap((res) => { this.session.set(res); this.persist(res); }),
    map(() => true),
    catchError(() => of(false)),
  );
}
```

`LoginComponent.submit()` then subscribes to `login(...)` instead of reading a boolean. The login request itself has no token yet, which is fine.

The existing interceptor already handles the API's 401 (sign out and redirect) and 403 (Access denied) responses.

## Notes

- Data is in memory and resets on restart. Swap `DataStore` for EF Core for a real database.
- Passwords are plain text and the JWT key is in `appsettings.json`: demo only. Use hashed passwords and user-secrets or a key vault for anything real.
- Roles are enforced on the server with `[Authorize(Roles = ...)]`, so hiding menu items in Angular is not the only protection.

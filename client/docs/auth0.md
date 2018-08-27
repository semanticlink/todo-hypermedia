
# Prerequisites

* Auth0 account

# Create Application (Single Page Application)

1. Login to [Auth0](https://manage.auth0.com)
2. Applications
3. Create Application
4. Single Page Application > Create
5. Settings (that need to be changed from defaults):
    * *Name:* Todo App
    * *Allowed Callback Urls:* `http://localhost:8080, http://localhost:5000, https://api.todo.semanticlink.io, http://localhost:63342, https://todo.semanticlink.io`
    * *Allowed Web Origins:* `http://localhost:8080, http://localhost:5000, https://api.todo.semanticlink.io, http://localhost:63342, https://todo.semanticlink.io`

Note: Allowed Origins (CORS) are automatically included based on the Callback URLs

From there you now have:

* Domain
* ClientID

Note: Client Secret is not used because we don't talk between the server and auth0

# Update server settings

* In `api/appsetting.json`:

```appsettings.json
  "Auth0": {
    "Domain": "semanticlink.au.auth0.com",
    "ClientID": "3LWYV9vifRV6ISnLVD06Xaxl23Nri8t1",
    "Audience": ...
  }
```

### Urls explained


* `http://localhost:8080`: dev todo app (from web dev server in Webstorm)
* `http://localhost:5000`: api (resources served from kestrel in Rider)
* `http://localhost:63342`: todo (standalone build in Webstorm)
* `https://api.todo.semanticlink.io` (live demo)
* `https://todo.semanticlink.io` (live demo)

# Create API

1. Login to [Auth0](https://manage.auth0.com)
2. APIs
3. Create API
5. Settings (that need to be changed from defaults):
    * *Name:* Todo
    * *Identifier:* `todo.semanticlink.io`

# Update server settings

* In `api/appsetting.json`, add the `Identifier` to 'Audience0:'

```appsettings.json
  "Auth0": {
    "Domain": ...,
    "ClientID": ...,
    "Audience": "todo.semanticlink.io"
  }
```

# Create and link a user

1. Login to [Auth0](https://manage.auth0.com)
2. Users
3. Create User
4. Settings (that need to be changed from defaults):
    * Name: test-1@semanticlink.io
    * Password: 1234qwerZXCV

This user should now be able to login to the application once it is redeployed.

Note: take a note of the `user_id` (eg auth0|5b8337f55351f52ac84f249a) as this would need to be used in the test data setup
and be linked to a test set of tenant and todo data. To recreate this data delete the database tables and restart the app.

```appsettings.json
  "Auth0": {
    ...
  },
  "TestSeedUser": "auth0|5b8337f55351f52ac84f249a",
```

## References

* [Good video of this process and click tutorials](https://manage.auth0.com/#/applications)
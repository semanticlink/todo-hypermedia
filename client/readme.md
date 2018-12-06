Instructions to build the todo app and generic api browser javascript code. This the Vue repository. These clients are to be used in conjunction with the todo [API]() codebase.

This repository includes sample code and base utility libraries:

* todo app
* generic API browser
* semantic-link-cache (client side application cache in javascript)
* semantic-link-utils (supporting general purpose utils that are not specific to the todo domain)

> Note: these libraries may at some stage be separated out but for now are ongoing development and published out of here

## Todo app
![](todo-app.png)

## Api browser
![](api-browser.png)

## Todo admin
![](todo-admin.png)

# Development

* [WebStorm](https://www.jetbrains.com/webstorm/download/)
* [nodejs](https://nodejs.org/en/) (>= 10.0.0)
* [yarn](https://yarnpkg.com/latest.msi) (1.6.0 or above)
* [Git for windows](https://git-scm.com/download/win) (2.15) (Adjust your PATH environment to use git and optional Unix tools from Windows Command Prompt)
* [deployment instructions in wiki](https://github.com/semanticlink/todo-hypermedia/wiki/Home)

## Prerequisites 

* JetBrains (Webstorm)
   - [Plantuml](https://plugins.jetbrains.com/plugin/7017?pr=idea) - `View > Tool Button` (and ensure `Graphviz` is setup with through Plantuml settings)
   - [Github markdown (gfm)](https://plugins.jetbrains.com/plugin/7701?pr=idea)
   - [IDEA mind map](https://plugins.jetbrains.com/plugin/8045-idea-mind-map)
   
* [Graphviz/dot](http://www.graphviz.org/)

## Optional

* Docker
* gfm

# Start

Ensure that the Api is running and then `yarn dev` will open browser in localhost:8080

```
cd client
yarn install
yarn dev
```

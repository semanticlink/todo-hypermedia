import Vue from 'vue';
import VueRouter from 'vue-router';
import {makeAbsolute, toSitePath} from 'semantic-link-utils/UriMapping';
import Admin from './components/app/Admin.vue';
import Todo from './components/app/Todo.vue';
import SelectTodo from './components/app/SelectTodo.vue';
import * as link from 'semantic-link';

Vue.use(VueRouter);

/**
 * Resolves the short-form in-browser-location Api Uri to CurrentUri on the component.
 *
 * Note: each component explicitly must add a 'props' to the component.vue. We then get the
 *       property on `this.apiUri`.
 *
 *  @example:
 *
 *  export default {
 *     props: {
 *         apiUri: {type: String}
 *     },
 *     methods: {
 *         goHome(){
 *            this.$router.push(toSitePath(this.apiUri, '/home/a/'));
 *         }
 *    }
 *  }
 *
 *  Then it can be used in a component:
 *
 *  @example
 *
 *      this.$router.push(toSitePath(this.currentUri, '/about/a/'));
 *    --> #/home/a/tenant/4
 *
 *  Note: you can use the router internal mechanism resolve the view but it will URL encode
 *  the URI. The recommended solution is to use above. See https://github.com/vuejs/vue-router/issues/787
 *
 *  @example
 *
 *      this.$router.push({ name: 'About', params: { apiUri: makeRelative(this.currentUri) }});
 *    --> #/home/a/tenant%2F4
 *
 *  TODO: we could clean this up and centralise the mapping between a view name and site prefix
 *        eg Home --> /home/a/
 *
 * @param {Route} route vue router
 * @return {{apiUri: string}} absolute uri of the current api
 */
function resolve(route) {
    if (route.params.apiUri) {
        return {apiUri: makeAbsolute(route.params.apiUri)};
    }
}

/**
 * Basic construction of the prefix for client-side of the uri that pivots on the '#' and the api uri
 *
 * @example https://example.com/#/todo/a/tenant/1
 *
 * We need to construct the `/todo/a/` part being the client prefix. The input is simply the unique
 * client-side routing, in this case 'todo'. In other cases of nested resources it will be more complex (eg `tenant/todo`)
 *
 *
 * @param clientPath
 * @returns {string}
 */
const makePrefix = clientPath => `/${clientPath}/a/`;

/**
 * Basic construction of routing path that concatenates the client-side view state with a wildcard that allows
 * us to get access to the api uri.
 *
 * @example https://example.com/#/todo/a/tenant/1
 *
 * We need to construct a `/todo/a/:apiUri(.*)` so that routing matches the client-side component and then passes in the
 * api uri (in this case `tenant/1` which is in turn mapped back the absolute api uri {@link resolve})
 *
 * @param clientPath
 * @returns {string}
 */
const makePath = clientPath => `${makePrefix(clientPath)}:apiUri(.*)`;

const clientPath = {
    Todo: 'todo',
};

export const routeName = {
    Todo: 'Todo',
    Home: 'Home',
    Admin: 'Admin',
};

export const routePath = {
    Home: '/',
    Admin: '/admin',
};

const router = new VueRouter({
    routes: [
        {
            path: routePath.Home,
            name: routeName.Home,
            component: SelectTodo,
        },
        {
            path: routePath.Admin,
            name: routeName.Admin,
            component: Admin,
        },
        {
            path: makePath(clientPath.Todo),
            name: routeName.Todo,
            component: Todo,
            props: resolve
        }
    ]
});

const redirect = (representation, path, query = {}) => router.push({...{path: toSitePath(link.getUri(representation, /self/), path)}, ...query});


export const redirectToTodo = (todoRepresentation, filter) => {
    return redirect(todoRepresentation, makePrefix(clientPath.Todo), filter);
};

export default router;

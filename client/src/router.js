import Vue from 'vue';
import VueRouter from 'vue-router';
import { makeAbsolute, toSitePath } from './lib/util/UriMapping';
import Home from './components/Home.vue';
import SelectTenants from './components/SelectTenants.vue';
import User from './components/User.vue';
import { SemanticLink } from "semanticLink";

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
        return { apiUri: makeAbsolute(route.params.apiUri) };
    }
}

let router = new VueRouter({
    routes: [
        {
            path: '/',
            name: 'Home',
            component: Home,
        },
        {
            path: '/tenants/a/:apiUri(.*)',
            name: 'SelectTenants',
            component: SelectTenants,
            props: resolve
        },
        {
            path: '/user/a/:apiUri(.*)',
            name: 'User',
            component: User,
            props: resolve
        }
    ]
});

const redirect = (representation, path) => router.push(toSitePath(SemanticLink.getUri(representation, /self/), path));

const redirectToTenant = tenantRepresentation => redirect(tenantRepresentation, '/tenant/a/');
const redirectToUser = userRepresentation => redirect(userRepresentation, '/user/a/');

export { redirectToTenant, redirectToUser };
export default router;

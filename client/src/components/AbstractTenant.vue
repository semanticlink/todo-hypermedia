<script>
    import { link, SemanticLink } from 'semanticLink';
    import { log } from 'logger';

    /**
     * Watches and waits for the tenant to be loaded into the network of data
     * so that we can display the logo and user correctly on the page.
     *
     * Partly this is a minimal implementation to see how well the notion of an
     * abstract component works. This seems easier to reason about than $emit/EventBus.
     *
     * This controller is registered in {@link main}
     *
     * Note: we could also look at
     */
    export default {
        name: 'AbstractTenant',
        abstract: true,
        data() {
            return {
                /**
                 * company logo to be displayed
                 */
                logo: undefined,
                tenant: undefined,
                user: undefined,
            };
        },
        render() {

            const vm = this;
            const waitForMyAccountToBeLoaded = () => setTimeout(
                () => {
                    const tenants = vm.$root.$api.tenants;
                    if (tenants && tenants.items) {
                        const tenant = _(tenants.items).first();
                        if (tenant && tenant.name && tenant.myAccount && tenant.myAccount.name) {
                            this.tenant = tenant.name;
                            this.user = tenant.myAccount.name;
                            link.get(tenant, /logo/)
                                .then(() => {
                                    vm.logo = SemanticLink.tryGetUri(tenant, /logo/) || '';
                                });

                        } else {
                            waitForMyAccountToBeLoaded();
                        }
                    } else {
                        waitForMyAccountToBeLoaded();
                    }
                }, 5);

            log.debug('Loading tenant');
            waitForMyAccountToBeLoaded();
        }
    };
</script>

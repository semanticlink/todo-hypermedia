<template>
    <div>
        <h1>Company:
            <draggable-model
                    :model="company"
                    :msg="company.name">
            </draggable-model>
            <droppable-model
                    :context="company"
                    :dropped="createOnCompany">
            </droppable-model>
            (user only)
        </h1>

        <h2>
            Zone Groups
        </h2>
        <ul>
            <li v-for="zonegroup in zonegroups.items">
                {{ zonegroup.name }}
            </li>
        </ul>


        <h2>Zones</h2>
        <ul>
            <li v-for="zone in zones.items">
                <a v-on:click="gotoZone(zone)">
                    {{ zone.name || 'unknown'}}
                </a>
                <draggable-model
                        :model="zone"
                        :msg="zone.name">
                </draggable-model>

            </li>
        </ul>

        <h2>Users</h2>
        <ul>
            <li v-for="user in users.items">
                <a v-on:click="gotoUser(user)">
                    {{ user.name || user.site || 'unknown'}}
                </a>
                <draggable-model
                        :model="user"
                        :msg="user.name">
                </draggable-model>
                ({{user.identity_type}})
            </li>
        </ul>

        <h2>Sites</h2>
        <ul>
            <li v-for="site in sites.items">
                <a v-on:click="gotoSite(site)">
                    {{ site.name || 'unknown'}}
                </a>
                <draggable-model
                        :model="site"
                        :msg="site.name">
                </draggable-model>

            </li>
        </ul>

    </div>
</template>

<script>

    import { log, nodMaker, SemanticLink } from 'semanticLink';
    import { nodSynchroniser } from 'semanticLink/NODSynchroniser';
    import { toSitePath } from '../lib/util/UriMapping';

    export default {
        name: 'Company',
        props: {
            apiUri: {type: String}
        },
        data () {
            return {
                company: {},
                users: {},
                sites: {},
                zones: {},
                zonegroups: {}
            };
        },
        created: function () {

            log.debug(`Loading selected organisation`);

            /**
             * use the organisation from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            const getCompanyUsersAndSites = (apiResource, nextStrategy = () => {}) => {
                log.debug('Looking for companies in', this.apiUri);

                /*
                 * This may error, at this point it is likely that it is not authentiated. If the
                 * 401 interception is broken then it will fall through to the catch and the err
                 * will contain rather than an error the actual resource.
                 *
                 */
                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getSingletonResource(apiResource, 'me', /me/)
                        .then(() => apiResource))
                    .then(apiResource => nodMaker.getCollectionResource(apiResource, 'organisations', /organisations/))
                    .then(organisations => nodMaker.getCollectionResource(organisations, 'companies', /companies/))
                    .then(companies => nodMaker.getCollectionResourceItemByUri(companies, this.apiUri))
                    .then(company => {
                        this.company = company;
                        return Promise.all([
                            nodMaker.getNamedCollectionResource(company, 'users', /users/)
                                .then(users => {
                                    this.users = users;
                                    // TODO: this does NOT propagate unique hydration changes to the UI in Vue
                                    //       implement such that each fetch has an UI update and you see the lazy loading
                                    //       happening on the screen
                                    //return nodMaker.getCollectionResourceAndItems(users);
                                }),
                            nodMaker.getNamedCollectionResource(company, 'sites', /sites/)
                                .then(sites => { this.sites = sites;}),
                            nodMaker.getNamedCollectionResource(company, 'zones', /zones/)
                                .then(zones => { this.zones = zones;}),
                            nodMaker.getNamedCollectionResource(company, 'zonegroups', /zone_groups/)
                                .then(zonegroups => { this.zonegroups = zonegroups;})
                        ]);
                    })
                    .catch(err => {
                        log.error(err);
                        return nextStrategy();
                    });
            };

            return getCompanyUsersAndSites(this.$root.$api);

        },
        methods: {
            createOnCompany (userDocument, companyRepresentation) {
                log.debug(`Create on company: '${userDocument.name}' --> ${SemanticLink.getUri(companyRepresentation, /self/)}`);

                // TODO: move beyond user

                nodSynchroniser.getResourceInNamedCollection(companyRepresentation, 'users', /users/, userDocument, [], {})
                    .then(resource => {
                        log.debug(resource);
                    })
                    .catch(err => {
                        log.error(err);
                    });
            },
            gotoUser (userRepresentation) {
                this.$router.push(toSitePath(SemanticLink.getUri(userRepresentation, /self/), '/user/a/'));
            },
            gotoSite (siteRepresentation) {
                this.$router.push(toSitePath(SemanticLink.getUri(siteRepresentation, /self/), '/site/a/'));
            }
        }
    };
</script>

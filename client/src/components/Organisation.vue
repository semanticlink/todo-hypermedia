<template>
    <div>
        <h1>
            {{ organisation.name }}
            <droppable-model
                    :context="organisation"
                    :dropped="createCompany">
            </droppable-model>
        </h1>

        <h2>Companies</h2>
        <ul>
            <li v-for="company in companies.items">
                <a v-on:click="gotoCompany(company)">
                    <draggable-model
                            :model="company"
                            :msg="company.name">
                    </draggable-model>
                </a>


                <drag-and-droppable-model
                        :msg="company.name"
                        :model="company"
                        :context="organisation"
                        :dropped="updateOrganisationWithCompany">

                </drag-and-droppable-model>
            </li>
        </ul>


        <h2>
            Regions
            <droppable-model
                    :context="organisation"
                    :dropped="createOrUpdateRegion">
            </droppable-model>
        </h2>
        <ul>

            <li v-for="region in regions.items">
                {{ region.name }}
                <drag-and-droppable-model
                        :msg="region.name"
                        :model="region"
                        :context="organisation"
                        :dropped="createOrUpdateRegion">
                </drag-and-droppable-model>
            </li>
        </ul>

        <h2>Users</h2>
        <ul>
            <li v-for="user in users.items"> {{ user }}
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
        name: 'Organisation',
        props: {
            apiUri: {type: String}
        },
        data () {
            return {
                organisation: {},
                companies: [],
                users: {},
                sites: {},
                regions: {}
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
            const getOrganisationCompanies = (apiResource, nextStrategy = () => {}) => {
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
                    .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /organisations/))
                    .then(organisations => nodMaker.getCollectionResourceItemByUri(organisations, this.apiUri))
                    .then(organisation => {
                        this.organisation = organisation;

                        return Promise.all([
                            nodMaker.getNamedCollectionResourceAndItems(organisation, 'companies', /companies/)
                                .then(companies => { this.companies = companies; }),
                            nodMaker.getNamedCollectionResource(organisation, 'users', /users/)
                                .then(users => {this.users = users;}),
                            nodMaker.getNamedCollectionResource(organisation, 'sites', /sites/)
                                .then(sites => { this.sites = sites;}),
                            nodMaker.getNamedCollectionResource(organisation, 'regions', /regions/)
                                .then(regions => {this.regions = regions;})
                        ]);
                    })
                    .catch(err => {
                        log.error(err);
                        return nextStrategy();
                    });
            };

            return getOrganisationCompanies(this.$root.$api);

        },
        methods: {
            updateOrganisationWithCompany (representation, context) {
                log.debug(`Sync ready: ${SemanticLink.getUri(representation, /self/)} --> ${SemanticLink.getUri(context, /self/)}`);
            },
            createCompany (representation, context) {
                log.debug(`Create company: '${representation.name}' --> ${SemanticLink.getUri(context, /self/)}`);
            },
            createOrUpdateRegion (representation, context) {
                nodSynchroniser.getResourceInNamedCollection(context, 'regions', /regions/, representation, [])
                    .catch(err => {
                        log.error(err);
                    });
            },
            gotoCompany (company) {
                this.$router.push(toSitePath(SemanticLink.getUri(company, /self/), '/organisation/company/a/'));
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

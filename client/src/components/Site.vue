<template>
    <div>
        <h1>Site:
            <drag-and-droppable-model
                    :model="site"
                    :context="site"
                    :dropped="createOnSite">
            </drag-and-droppable-model>
        </h1>
        {{ site.zones}}
        <div v-for="(value, key) in displayFieldsForSite">
            <i>{{ key }}:</i> {{ value }}
        </div>

        <h2>Address</h2>
        <div v-for="(value, key) in displayFieldsForAddress">
            <i>{{ key }}:</i>: {{ value }}
        </div>

        <h2>Region</h2>
        <div v-for="(value, key) in displayFieldsForRegion">
            <i>{{ key }}:</i>: {{ value }}
        </div>

        <h2>
            Zones
            <drag-and-droppable-model
                    :context="site"
                    :model="site"
                    :dropped="createOrUpdateZoneGroupOnSite">
            </drag-and-droppable-model>
        </h2>

        <zone-menu
                :nodes="site"
                :label="zonesTitle"
                :type="'root'"
                :depth="0"/>


    </div>
</template>

<script>

    import { _, log, nodMaker, SemanticLink } from 'semanticLink';
    import { nodSynchroniser } from 'semanticLink/NODSynchroniser';

    export default {
        name: 'Site',
        props: {
            apiUri: {type: String}
        },
        data () {
            return {
                /**
                 * Context for updating a site
                 */
                company: {},
                site: {},
                addressRel: {},
                region: {},
                zonesTitle: 'Zones'
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
            const getSiteWithRegionsAndZonesAndAddress = (apiResource, nextStrategy = () => {}) => {
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
                    .then(companies => nodMaker.getCollectionResource(companies, 'sites', /sites/))
                    .then(sites => nodMaker.getCollectionResourceItemByUri(sites, this.apiUri))
                    .then(site => {
                        this.site = site;

                        /**
                         *  WARNING: a site representation has a deprecated attribute 'address' that conflicts
                         *           with the link relation 'address'. The error reported is 'No resource to find state on'
                         *           when it looks up the attribute (correctly). Until address attribute
                         *           is removed. Delete from model.
                         **/
                        //delete site.address;

                        return Promise.all([
                            nodMaker.getSingletonResource(site, 'addressRel', /address/)
                                .then(address => {this.addressRel = address;}),
                            // TODO: 'region' can have multiple link rels and we need to deal with this as a collection of singletons
                            /**
                             * @example
                             *  {
                             *      rel: "region",
                             *      href: "http://localhost:8002/region/3"
                             *  },
                             *  {
                             *      rel: "region",
                             *      href: "http://localhost:8002/region/16",
                             *      title: "Police"
                             *  },
                             */
                            nodMaker.getSingletonResource(site, 'region', /region/)
                                .then(region => {this.region = region;}),
                            // TODO: 'zones' can have multiple link rels and we need to deal with this as a collection of collections
                            /**
                             * @example
                             *  {
                             *      rel: "zones",
                             *      href: "http://localhost:8002/site/2/zone"
                             *  },
                             *  {
                             *      rel: "zones",
                             *      href: "http://localhost:8002/site/2/zone/all",
                             *      title: "all"
                             *  },
                             */
                            nodMaker.getNamedCollectionResource(site, 'zones', /zones/),
                            nodMaker.getNamedCollectionResourceAndItems(site, 'zonegroups', /zone_groups/)
                                .then(zonegroups => {
                                    return nodMaker.tryGetNamedCollectionResourceAndItemsOnCollectionItems(zonegroups, 'zones', /zones/);
                                })
                        ]);
                    })
                    .catch(err => {
                        log.error(err);
                        return nextStrategy();
                    });
            };

            return getSiteWithRegionsAndZonesAndAddress(this.$root.$api);

        },
        computed: {
            displayFieldsForSite: function () {
                return _(this.site).omit('links', 'id', 'zones', 'region', 'address', 'zonegroups');
            },
            displayFieldsForRegion: function () {
                return _(this.region).omit('links', 'id');
            },
            displayFieldsForAddress: function () {
                return _(this.addressRel).omit('links', 'id');
            }
        },
        methods: {
            createOnSite (siteDocument, siteRepresentation) {
                log.debug(`Update site: '${siteRepresentation.name}' --> ${SemanticLink.getUri(siteRepresentation, /self/)}`);

                function syncAddressStrategy (siteRepresentation, siteDocument, options) {
                    return nodSynchroniser.getSingleton(siteRepresentation, 'addressRel', /address/, siteDocument, [], options);
                }

                return nodSynchroniser
                    .getResource(
                        siteRepresentation,
                        siteDocument,
                        [
                            (siteRepresentation, siteDocument, options) => syncAddressStrategy(siteRepresentation, siteDocument, options)
                        ],
                        {})
                    .catch(err => { log.error(err);});

            },
            createOrUpdateZoneGroupOnSite (aZoneGroup, siteRepresentation) {

                /**
                 * TODO: implement as recursive structure
                 */

                function syncZoneGroupsStrategy (siteRepresentation, zoneGroupDocument, strategies, options) {
                    return nodSynchroniser.getResourceInNamedCollection(siteRepresentation, 'zonegroups', /zone_groups/, zoneGroupDocument, strategies, options);
                }

                function syncZonesStrategy (zoneGroupRepresentation, zoneGroupDocument, options) {
                    return nodSynchroniser.getNamedCollection(zoneGroupRepresentation, 'zones', /zones/, zoneGroupDocument, [], options);
                }

                return nodMaker
                    .getResource(siteRepresentation)
                    .then(site => syncZoneGroupsStrategy(
                        site,
                        aZoneGroup,
                        [
                            (zoneGroupResource, zoneGroupDocument, options) => syncZonesStrategy(zoneGroupResource, zoneGroupDocument, options)
                        ],
                        {}))
                    .catch(err => {
                        log.error(err);
                    });

            }

        }
    };
</script>

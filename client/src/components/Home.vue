<template>
    <div class="hello">
        <p>Hello {{ me.user_name }}</p>

        <h1>Organisations
            <droppable-model
                    :context="$root.$api"
                    :dropped="createOrUpdateOrganisationOnRoot">
            </droppable-model>
        </h1>

        <ul>
            <li v-for="organisation in organisations.items">
                <a v-on:click="gotoOrganisation(organisation)">{{ organisation.name }}</a>
                <drag-and-droppable-model
                        :msg="organisation.name"
                        :model="organisation"
                        :context="$root.$api"
                        :dropped="createOrUpdateOrganisationOnRoot">
                </drag-and-droppable-model>
            </li>
        </ul>
    </div>
</template>

<script>
    import { log, nodMaker, SemanticLink } from 'semanticLink';
    import { toSitePath } from '../lib/util/UriMapping';
    import { nodSynchroniser } from 'semanticLink/NODSynchroniser';

    export default {
        name: 'Hello',
        data () {
            return {
                msg: 'Looking ...',
                error: false,
                invalid: '',
                busy: true,
                me: {},
                organisations: {},
            };
        },
        created: function () {

            log.info(`Loading selected tenant from api`);

            /**
             * Strategy One: use the first tenant from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            const strategyOneProvidedTenant = (apiResource, nextStrategy = () => {}) => {
                log.info('Looking for provided tenant');

                /*
                 * This may error, at this point it is likely that it is not authentiated. If the
                 * 401 interception is broken then it will fall through to the catch and the err
                 * will contain rather than an error the actual resource.
                 *
                 * TODO: improve
                 */
                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getSingletonResource(apiResource, 'me', /me/))
                    .then(me => {
                        this.me = me;
                        return nodMaker.tryGetCollectionResourceAndItems(apiResource, 'organisations', /organisations/);
                    })
                    .then(organisations => {
                        this.organisations = organisations;
                        return nextStrategy();
                    })
                    .catch(err => {
                        log.error(err);
                        return nextStrategy();
                    });
            };

            return strategyOneProvidedTenant(this.$root.$api);

        },
        methods: {
            gotoOrganisation (organisation) {
                this.$router.push(toSitePath(SemanticLink.getUri(organisation, /self/), '/organisation/a/'));
            },
            createOrUpdateOrganisationOnRoot (organisationDocument, rootRepresentation) {
                nodSynchroniser.getResourceInNamedCollection(rootRepresentation, 'organisations', /organisations/, organisationDocument, [])
                    .then(resource => {
                        log.debug(resource);
                    })
                    .catch(err => {
                        log.error(err);
                    });
            }

        },
    };
</script>

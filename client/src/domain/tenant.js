import { nodMaker, link, _ } from "semanticLink";
import { log } from 'logger';
import SparseResource from "semanticLink/SparseResource";

/**
 * Search for a tenant using the search form on the collection
 *
 * @param {TenantCollectionRepresentation} tenantCollection
 * @param tenantName
 * @returns {Promise.<CollectionRepresentation>} containing the search result collection
 */
const searchForTenant = (tenantCollection, tenantName) => {

    if (!tenantName){
        return Promise.resolve();
    }

    log.debug('Looking for search form on tenant collection');

    return nodMaker.getSingletonResource(tenantCollection, 'searchForm', /search/)
        .then(tenantSearchRepresentation => {

            let form = _(tenantSearchRepresentation.items).first();

            if (form) {

                /**
                 * Construct search based on the tenantSearchRepresentation. This is a simple form
                 * so we just grab the first item and haven't checks on type.
                 *
                 * In the future, we might ask for a field which is http://types/text
                 */
                const searchForm = {};
                searchForm[form.name] = tenantName;

                return link.post(tenantSearchRepresentation, /submit/, 'application/json', searchForm);
            } else {
                throw new Error(`Search form item does not exist on ${link.getUri(tenantCollection, /search/)}. Do you have the correct headers set?`);
            }
        })
        .then(createdResult => link.http.get(createdResult.headers.location))
        .then(searchResult =>  SparseResource.mapFeedItemsToCollectionItems(searchResult.data));
};

const getTenants = apiResource => nodMaker
    .getResource(apiResource)
    .then(apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/));

export { searchForTenant, getTenants };
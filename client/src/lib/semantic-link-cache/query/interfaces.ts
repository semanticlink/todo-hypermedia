import {CollectionRepresentation, LinkedRepresentation, RelationshipType, Uri} from "semantic-link";
import {FormRepresentation, UriList} from "../interfaces";

export type Representation = (CollectionRepresentation | FormRepresentation | UriList | any) & LinkedRepresentation;

/**
 * Options for be able to traverse the semantic network of data
 */
export type QueryOptions = {
    /**
     * If this is set then the get will perform a `tryGet` and return default representation on failure
     */
    defaultRepresentation?: Representation,
    /**
     * Identifies the child resource in a collection by its identity (either as 'self' link rel or a Uri)
     */
    where?: Representation | Uri,
    /**
     * Identifies the link rel to follow to add {@link LinkedRepresentation} onto the resource.
     */
    rel?: RelationshipType,
    /**
     * The name of the attribute that the {@link LinkedRepresentation} is added on the resource. Note: this
     * value is defaulted based on the {@link QueryOptions.rel} if not specified. If the {@link QueryOptions.rel} is
     * an array then this value must be explicitly set.
     */
    name?: string
    /**
     * Alters the hydration strategy for collections. By default collections are sparsely populated (that is
     * the `items` attribute has not gone to the server to get all the details for each item).
     * {@link QueryOptions.include} currently flags that it should go and fetch each item.
     */
    includeItems?: boolean
} & {} | any;

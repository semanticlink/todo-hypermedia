import {Representation} from "../query/interfaces";
import {RelationshipType} from "semantic-link";

/**
 * TODO: link back up with UtilOptions
 */
export interface SyncOptions {
}

export type StrategyType = <T extends Representation>(resource: T, document: T, options: SyncOptions) => Promise<void>;

export interface ResourceSync<T extends Representation> {
    resource: T;
    document: T;
    strategies?: StrategyType[];
    options?: SyncOptions;
}

export interface NamedResourceSync<T extends Representation> extends ResourceSync<T> {
    /**
     * Link rel on the parent (ie context) resource to be followed
     */
    rel: RelationshipType;
    /**
     * Link rel used on the document resource to find context. This is used in the scenario of syncing a named collection
     * on a named collection. Usually this value will be the same as the {@link rel} value.
     *
     * @remarks
     * Decided not to make this also a boolean value where it then takes the {@link rel} value.
     */
    documentRel?: string;
    /**
     * The attribute name of the named resource that is added to the in-memory resource. This is an override value
     * where the default is a stringly-type of {@link rel}.
     */
    name?: string;
}


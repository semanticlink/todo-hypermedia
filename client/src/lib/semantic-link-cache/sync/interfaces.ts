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
     * The attribute name of the named resource that is added to the in-memory resource. This is an override value
     * where the default is a stringly-type of {@link rel}.
     */
    name?: string;
}


/**
 * This is a documentation file of the client-side artifact representations
 */

import {LinkedRepresentation, CollectionRepresentation} from 'semantic-link';

export interface ApiRepresentation extends LinkedRepresentation {
    version: string;
}

export interface TenantCollectionRepresentation extends CollectionRepresentation {

}

export interface TenantRepresentation extends LinkedRepresentation {
    name: string;
}

export interface TenantSearchRepresentation extends LinkedRepresentation {
    search: LinkedRepresentation[];
}

export interface TodoCollectionRepresentation extends CollectionRepresentation {

}

export interface TodoRepresentation extends LinkedRepresentation {
    name: string;
}

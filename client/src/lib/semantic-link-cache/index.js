import * as link from 'semantic-link';
import * as cache from './cache';
import * as query from './query';
import {LogLevel as LEVEL, setLogLevel, log} from 'logger';
import _ from './mixins/index';
import {uriMappingResolver} from './sync/UriMappingResolver';
import Loader, {loader} from './Loader';
import PooledCollection from './sync/PooledCollection';

const LoaderEvent = Loader.event;

export {sync} from './sync';

export {get, update, del, create} from './query';

export {
    link,
    cache,
    query,
    _,
    log,
    setLogLevel,
    LEVEL,
    uriMappingResolver,
    loader,
    LoaderEvent,
    PooledCollection
};
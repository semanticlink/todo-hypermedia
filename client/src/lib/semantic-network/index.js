import * as link from 'semantic-link';
import * as cache from './cache';
import * as query from './query';
import {LogLevel as LEVEL, setLogLevel, log} from 'logger';
import _ from './mixins/index';
import {uriMappingResolver} from './sync/UriMappingResolver';
import Loader, {loader} from './loader/Loader';
import {sync} from './sync';
import {get, update, del, create} from './query';

const LoaderEvent = Loader.event;


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
    get,
    update,
    del,
    del as delete,
    create,
    sync
};
import * as link from 'semantic-link';
import * as cache from './cache';
import * as sync from './sync';
import {LogLevel as LEVEL, setLogLevel, log} from 'logger';
import _ from './mixins/index';
import {uriMappingResolver} from './sync/UriMappingResolver';
import Loader, {loader} from './Loader';

const LoaderEvent = Loader.event;

export {
    link,
    cache,
    sync,
    _,
    log,
    setLogLevel,
    LEVEL,
    uriMappingResolver,
    loader,
    LoaderEvent
};
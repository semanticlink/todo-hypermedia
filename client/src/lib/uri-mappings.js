import { uriMapping } from './util/UriMapping';
import { log } from 'semanticLink/index';

export const apiUri = document.querySelector('HEAD link[rel="api"]').href;
export const authenticatorUri = document.querySelector('HEAD link[rel="authenticator"]').href;
export const clientUri = window.location.href;

log.info(`Client uri: '${clientUri}'`);
log.info(`Api uri: '${apiUri}'`);

uriMapping(clientUri, apiUri);


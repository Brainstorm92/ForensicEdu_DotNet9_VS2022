import {API_BASE_URL} from './apiTransport';
export function mediaImageUrl(value:string){return value.startsWith('/api/')?API_BASE_URL.replace(/\/$/,'')+value.slice(4):value;}

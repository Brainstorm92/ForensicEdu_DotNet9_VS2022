import {createContext,useCallback,useContext,useEffect,useRef,useState,type ReactNode} from 'react';
import {useAuth} from './AuthContext';
import {apiRequest} from '../services/apiTransport';
export type StudentNotice={id:string;kind:string;title:string;body:string;actionTab:string;createdAt:string;readAt:string|null};
export type NoticePage={items:StudentNotice[];total:number;unreadCount:number;page:number;pageSize:number};
const Context=createContext<{unread:number;refresh:()=>Promise<void>}|null>(null);
export function NotificationProvider({children}:{children:ReactNode}){
 const {user}=useAuth();const [unread,setUnread]=useState(0);const generation=useRef(0);
 const refresh=useCallback(async()=>{const version=generation.current;if(!user)return;try{const page=await apiRequest<NoticePage>('/student/notifications');if(version===generation.current)setUnread(page.unreadCount);}catch{/* Inbox shows its own actionable error; the header must not block other screens. */}},[user?.id]);
 useEffect(()=>{generation.current++;setUnread(0);void refresh();const timer=setInterval(()=>{if(!document.hidden)void refresh();},30000);const focus=()=>{if(!document.hidden)void refresh();};window.addEventListener('focus',focus);return()=>{generation.current++;clearInterval(timer);window.removeEventListener('focus',focus);};},[refresh]);
 return <Context.Provider value={{unread,refresh}}>{children}</Context.Provider>;
}
export function useNotifications(){const ctx=useContext(Context);if(!ctx)throw new Error('Falta NotificationProvider');return ctx;}

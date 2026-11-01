import {useEffect,type RefObject} from 'react';
// Keep the focused control visible when the mobile keyboard or orientation changes the viewport.
export function useDialogViewport(dialog:RefObject<HTMLElement|null>,enabled=true){
 useEffect(()=>{if(!enabled)return;let frame=0;const reveal=()=>{cancelAnimationFrame(frame);frame=requestAnimationFrame(()=>{const active=document.activeElement;if(active instanceof HTMLElement&&dialog.current?.contains(active)&&active.matches('input,select,textarea'))active.scrollIntoView({block:'nearest',inline:'nearest'});});};window.addEventListener('resize',reveal);window.visualViewport?.addEventListener('resize',reveal);return()=>{cancelAnimationFrame(frame);window.removeEventListener('resize',reveal);window.visualViewport?.removeEventListener('resize',reveal);};},[dialog,enabled]);
}

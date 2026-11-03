import {useEffect} from 'react';
let locks=0;let bodyOverflow='';let rootOverflow='';
// Shared counter prevents a nested dialog from unlocking the page underneath another dialog.
export function useDialogScrollLock(enabled:boolean){
 useEffect(()=>{if(!enabled)return;if(locks++===0){bodyOverflow=document.body.style.overflow;rootOverflow=document.documentElement.style.overflow;document.body.style.overflow='hidden';document.documentElement.style.overflow='hidden';}return()=>{if(--locks===0){document.body.style.overflow=bodyOverflow;document.documentElement.style.overflow=rootOverflow;}};},[enabled]);
}

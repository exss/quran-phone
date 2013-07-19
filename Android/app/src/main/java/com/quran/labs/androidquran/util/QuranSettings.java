package com.quran.labs.androidquran.util;

import android.content.Context;
import android.content.SharedPreferences;

import android.os.Build;
import android.preference.PreferenceManager;
import com.quran.labs.androidquran.data.Constants;


public class QuranSettings {

   public static boolean isArabicNames(Context context){
      return getBooleanPreference(context,
              Constants.PREF_USE_ARABIC_NAMES, false);
   }

   public static boolean isLockOrientation(Context context){
      return getBooleanPreference(context,
              Constants.PREF_LOCK_ORIENTATION, false);
   }

   public static boolean isLandscapeOrientation(Context context){
      return getBooleanPreference(context,
              Constants.PREF_LANDSCAPE_ORIENTATION, false);
   }

   public static boolean shouldStream(Context context){
      return getBooleanPreference(context,
              Constants.PREF_PREFER_STREAMING, false);
   }

   public static boolean needArabicFont(Context context){
      return Build.VERSION.SDK_INT < Build.VERSION_CODES.ICE_CREAM_SANDWICH;
   }

   public static boolean isReshapeArabic(Context context){
      boolean defValue =
              (Build.VERSION.SDK_INT < Build.VERSION_CODES.ICE_CREAM_SANDWICH);
      return getBooleanPreference(context, Constants.PREF_USE_ARABIC_RESHAPER,
              defValue);
   }

   public static boolean isNightMode(Context context){
      return getBooleanPreference(context,
              Constants.PREF_NIGHT_MODE, false);
   }

   public static boolean shouldOverlayPageInfo(Context context){
      return getBooleanPreference(context,
            Constants.PREF_OVERLAY_PAGE_INFO, true);
   }

   public static boolean shouldDisplayMarkerPopup(Context context){
      return getBooleanPreference(context,
              Constants.PREF_DISPLAY_MARKER_POPUP, true);
   }

   public static boolean wantArabicInTranslationView(Context context){
      return getBooleanPreference(context,
              Constants.PREF_AYAH_BEFORE_TRANSLATION, true);
   }

   private static boolean getBooleanPreference(Context context,
                                               String pref,
                                               boolean defaultValue){
      SharedPreferences prefs =
              PreferenceManager.getDefaultSharedPreferences(context);
      return prefs.getBoolean(pref, defaultValue);
   }

   public static int getPreferredDownloadAmount(Context context){
      SharedPreferences prefs =
              PreferenceManager.getDefaultSharedPreferences(context);
      String str = prefs.getString(Constants.PREF_DOWNLOAD_AMOUNT,
                  "" + AudioUtils.LookAheadAmount.PAGE);
      int val = AudioUtils.LookAheadAmount.PAGE;
      try { val = Integer.parseInt(str); }
      catch (Exception e){}

      if (val > AudioUtils.LookAheadAmount.MAX ||
              val < AudioUtils.LookAheadAmount.MIN){
         return AudioUtils.LookAheadAmount.PAGE;
      }
      return val;
   }

   public static int getTranslationTextSize(Context context){
      SharedPreferences prefs =
              PreferenceManager.getDefaultSharedPreferences(context);
      return prefs.getInt(Constants.PREF_TRANSLATION_TEXT_SIZE,
              Constants.DEFAULT_TEXT_SIZE);
   }

   public static void setLastPage(Context context, int page){
      SharedPreferences prefs =
              PreferenceManager.getDefaultSharedPreferences(context);
      prefs.edit().putInt(Constants.PREF_LAST_PAGE, page).commit();
   }
}

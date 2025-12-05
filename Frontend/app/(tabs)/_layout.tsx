import { View, Text } from 'react-native'
import React from 'react'
import { Tabs } from 'expo-router'

const _layout = () => {
  return (
    <Tabs
      screenOptions={{
        tabBarShowLabel: true,
      }}
    >
      <Tabs.Screen 
        name='home'
        options={{
          title: 'Home',
          headerShown: false
        }}
      />
      <Tabs.Screen 
        name='community'
        options={{
          title: 'Communities',
          headerShown: false
        }}
      />
      <Tabs.Screen 
        name='profile'
        options={{
          title: 'Profile',
          headerShown: false
        }}
      />
      
    </Tabs>
  )
}

export default _layout
import { Text, View } from "react-native";
import {Link} from "expo-router"
export default function Index() {
  return (
    <View className="">
      <Link href="/sign_in"><Text className="text-2xl font-bold">Hello</Text></Link>
    </View>
  );
}

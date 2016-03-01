using UnityEngine;
using UnityEngine.Events;

namespace druggedcode
{
    public class MathUtil
    {
        static public bool GetRandomBool( float successPer = 0.5f )
        {
            if( Random.Range( 0f , 1f ) < successPer )
                return true;
            else
                return false;
        }

        static public void RandomExecute( params UnityAction[] funcs )
        {

            int count = funcs.Length;
            float quotient = 1 / (float)count;
            float ran = Random.Range( 0f , 1f );

            while( count-- > 0 )
            {
                if( ran > quotient  * count )
                {
                    funcs[ count ]();
                    break;
                }
            }
        }

        static public float ByteToMB( float fromByte, uint fix = 1)
        {
            return fromByte * 0.000000954f;
        }

        /// <summary>
        /// Round the specified num and roundInterval.
        /// 소수점 둘째자리로 반올림 - MathUtil.round(12.3234,0.01) //12.32
        /// 4의곱에 가깝게 반올림 - MathUtil.round(22.1,4) // 24
        /// 0.5 단위로 반올림  - MathUtil.round(24.271,0.5) // 24.5 
        /// </summary>
        static public float Round( float num, float roundInterval )
        {
            return Mathf.Round( num/ roundInterval) * roundInterval;
        }
        /*
        }
        //각도를 -360 ~ 360 사이의 값으로 변환 (500 -> 140,  -400 -> -40)
        public static function degreeParsing($degree:Number):Number
        {
            if (Math.abs($degree)<360)
                return $degree;
            else
            {
                if( $degree > 0 )
                    return $degree - 360;
                else
                    return $degree + 360;
            }
            
            return $degree;
        }
        
        //각도를 0~360 사이의 값으로 변환
        public static function degreeParsingUint( $degree: Number): Number
        {
            while ($degree < 0) {
                $degree += 360;
            }
            while ($degree >= 360) {
                $degree -= 360;
            }
            return $degree;
        }
        

        //360도를 $seg 로 분할 하여 $degree 의 index를 구한다.<br/>순서는 시계방향.<br/>
        // findAngleIndex( 0,4 ) > 0
        //findAngleIndex( 45,4 ) > 1
        //<code>findAngleIndex( 60,4 )</code> is 1
        //<code>findAngleIndex( 120,4 )</code> is 1
        //code>findAngleIndex( 135,4 )</code> is 2
        //<code>findAngleIndex( 280,4 )</code> is 3

        static public function findAngleIndex( $degree:Number, $seg:int = 4 ) :int
        {
            var margin: Number = 360/$seg;
            $degree = degreeParsingUint($degree + margin/2);
            return Math.floor($degree / margin);
        }
*/
    }
}


